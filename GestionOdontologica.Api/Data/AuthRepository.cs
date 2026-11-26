using System.Data;
using System.Security.Cryptography;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Data;

/// <summary>
/// Autenticación y recuperación de contraseña usando exclusivamente los
/// procedimientos almacenados independientes (sp_*). La verificación BCrypt se
/// hace en C#.
/// </summary>
public sealed class AuthRepository : IAuthRepository
{
    private const int CommandTimeoutSeconds = 60;
    private const int MaxIntentosFallidos = 5;
    private const int MinutosBloqueo = 15;
    private const int MinutosVigenciaToken = 30;

    private readonly IOracleConnectionFactory _connectionFactory;

    public AuthRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ResultadoInicioSesion> IniciarSesionAsync(
        string identificador,
        string password,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var credenciales = await ObtenerCredencialesAsync(
            connection,
            identificador.Trim(),
            cancellationToken);

        if (credenciales is null)
        {
            // No se revela si el usuario existe o no.
            return new ResultadoInicioSesion(EstadoInicioSesion.CredencialesInvalidas);
        }

        if (!credenciales.Activo)
        {
            return new ResultadoInicioSesion(
                EstadoInicioSesion.CuentaInactiva,
                IdUsuario: credenciales.IdUsuario);
        }

        if (credenciales.SegundosBloqueo > 0)
        {
            return new ResultadoInicioSesion(
                EstadoInicioSesion.CuentaBloqueada,
                SegundosBloqueo: credenciales.SegundosBloqueo,
                IdUsuario: credenciales.IdUsuario);
        }

        if (!PasswordHasher.Verify(password, credenciales.PasswordHash))
        {
            var (intentos, segundosBloqueo) = await RegistrarIntentoFallidoAsync(
                connection,
                credenciales.IdUsuario,
                cancellationToken);

            if (segundosBloqueo > 0)
            {
                return new ResultadoInicioSesion(
                    EstadoInicioSesion.CuentaBloqueada,
                    SegundosBloqueo: segundosBloqueo,
                    IdUsuario: credenciales.IdUsuario);
            }

            return new ResultadoInicioSesion(
                EstadoInicioSesion.CredencialesInvalidas,
                IntentosRestantes: Math.Max(MaxIntentosFallidos - intentos, 0),
                IdUsuario: credenciales.IdUsuario);
        }

        await RegistrarAccesoAsync(connection, credenciales.IdUsuario, cancellationToken);

        return new ResultadoInicioSesion(
            EstadoInicioSesion.Exitoso,
            new UsuarioDto
            {
                IdUsuario = credenciales.IdUsuario,
                NombreCompleto = credenciales.NombreCompleto,
                NombreUsuario = credenciales.NombreUsuario,
                Correo = credenciales.Correo,
                Rol = credenciales.Rol,
                Activo = true
            },
            IdUsuario: credenciales.IdUsuario);
    }

    public async Task<ResultadoSolicitudRecuperacion> SolicitarRecuperacionAsync(
        string correo,
        int minutosVigencia,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // El token se genera con un CSPRNG y solo se guarda su hash SHA-256 en
        // Oracle. La API lo devuelve una única vez al cliente.
        var token = RandomNumberGenerator.GetHexString(32);

        using var command = CrearProcedimiento("sp_solicitar_recuperacion", connection);

        AgregarEntrada(command, "p_correo", OracleDbType.Varchar2, 150, correo.Trim());
        AgregarEntrada(command, "p_token", OracleDbType.Varchar2, 64, token);
        AgregarEntrada(
            command,
            "p_minutos_vigencia",
            OracleDbType.Int32,
            null,
            minutosVigencia > 0 ? minutosVigencia : MinutosVigenciaToken);

        var existeSalida = command.Parameters.Add(
            "p_existe",
            OracleDbType.Int32,
            ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        var existe = existeSalida.GetInt32Value() == 1;

        return new ResultadoSolicitudRecuperacion(existe, existe ? token : null);
    }

    public async Task<bool> RestablecerPasswordAsync(
        string token,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_restablecer_password", connection);

        AgregarEntrada(command, "p_token", OracleDbType.Varchar2, 64, token.Trim());
        AgregarEntrada(command, "p_password_hash", OracleDbType.Varchar2, 255, passwordHash);

        var resultadoSalida = command.Parameters.Add(
            "p_resultado",
            OracleDbType.Int32,
            ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return resultadoSalida.GetInt32Value() == 0;
    }

    private static async Task<Credenciales?> ObtenerCredencialesAsync(
        OracleConnection connection,
        string identificador,
        CancellationToken cancellationToken)
    {
        using var command = CrearProcedimiento("sp_obtener_credenciales", connection);

        AgregarEntrada(command, "p_identificador", OracleDbType.Varchar2, 150, identificador);

        var cursor = command.Parameters.Add(
            "p_cursor",
            OracleDbType.RefCursor,
            ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        if (cursor.Value is not OracleRefCursor refCursor)
        {
            return null;
        }

        using var reader = refCursor.GetDataReader();

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Credenciales(
            LeerEntero(reader, "ID_USUARIO"),
            LeerTexto(reader, "NOMBRE_COMPLETO"),
            LeerTexto(reader, "NOMBRE_USUARIO"),
            LeerTexto(reader, "CORREO"),
            LeerTexto(reader, "ROL"),
            LeerEntero(reader, "ACTIVO") == 1,
            LeerTexto(reader, "PASSWORD"),
            LeerEntero(reader, "SEGUNDOS_BLOQUEO"));
    }

    private static async Task<(int Intentos, int SegundosBloqueo)> RegistrarIntentoFallidoAsync(
        OracleConnection connection,
        int idUsuario,
        CancellationToken cancellationToken)
    {
        using var command = CrearProcedimiento("sp_registrar_intento_fallido", connection);

        AgregarEntrada(command, "p_id_usuario", OracleDbType.Int32, null, idUsuario);
        AgregarEntrada(command, "p_max_intentos", OracleDbType.Int32, null, MaxIntentosFallidos);
        AgregarEntrada(command, "p_minutos_bloqueo", OracleDbType.Int32, null, MinutosBloqueo);

        var intentosSalida = command.Parameters.Add(
            "p_intentos",
            OracleDbType.Int32,
            ParameterDirection.Output);

        var segundosSalida = command.Parameters.Add(
            "p_segundos_bloqueo",
            OracleDbType.Int32,
            ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return (
            intentosSalida.GetInt32Value(),
            segundosSalida.GetInt32Value());
    }

    private static async Task RegistrarAccesoAsync(
        OracleConnection connection,
        int idUsuario,
        CancellationToken cancellationToken)
    {
        using var command = CrearProcedimiento("sp_registrar_acceso", connection);

        AgregarEntrada(command, "p_id_usuario", OracleDbType.Int32, null, idUsuario);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static OracleCommand CrearProcedimiento(string nombre, OracleConnection connection)
    {
        return new OracleCommand(nombre, connection)
        {
            CommandType = CommandType.StoredProcedure,
            BindByName = true,
            CommandTimeout = CommandTimeoutSeconds
        };
    }

    private static void AgregarEntrada(
        OracleCommand command,
        string nombre,
        OracleDbType tipo,
        int? tamanio,
        object? valor)
    {
        var parametro = tamanio is null
            ? command.Parameters.Add(nombre, tipo)
            : command.Parameters.Add(nombre, tipo, tamanio.Value);

        parametro.Value = valor ?? DBNull.Value;
    }

    private static string LeerTexto(OracleDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static int LeerEntero(OracleDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private sealed record Credenciales(
        int IdUsuario,
        string NombreCompleto,
        string NombreUsuario,
        string Correo,
        string Rol,
        bool Activo,
        string PasswordHash,
        int SegundosBloqueo);
}
