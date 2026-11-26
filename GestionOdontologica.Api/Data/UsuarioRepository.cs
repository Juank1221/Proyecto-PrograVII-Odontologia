using System.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Exceptions;
using GestionOdontologica.Api.Security;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Data;

/// <summary>
/// Repositorio de usuarios. Todas las operaciones se ejecutan a través de
/// procedimientos almacenados independientes (sp_*); la API no contiene SQL
/// directo contra las tablas. El hash BCrypt se calcula en C# porque Oracle no
/// lo implementa de forma nativa y los procedimientos solo persisten/retornan
/// el valor.
/// </summary>
public sealed class UsuarioRepository : IUsuarioRepository
{
    private const int CommandTimeoutSeconds = 60;

    private readonly IOracleConnectionFactory _connectionFactory;

    public UsuarioRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_usuarios", connection);
        var cursor = AgregarCursorSalida(command);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return LeerUsuarios(cursor);
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await ConsultarPorIdAsync(connection, id, cancellationToken);
    }

    public async Task<UsuarioDto> InsertarAsync(
        CreacionUsuarioDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var passwordHash = PasswordHasher.Hash(dto.Password);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        int idCreado;

        try
        {
            using var command = CrearProcedimiento("sp_insertar_usuario", connection);

            AgregarEntrada(command, "p_nombre_completo", OracleDbType.Varchar2, 150, dto.NombreCompleto.Trim());
            AgregarEntrada(command, "p_nombre_usuario", OracleDbType.Varchar2, 50, dto.NombreUsuario.Trim());
            AgregarEntrada(command, "p_correo", OracleDbType.Varchar2, 150, dto.Correo.Trim());
            AgregarEntrada(command, "p_password", OracleDbType.Varchar2, 255, passwordHash);
            AgregarEntrada(command, "p_rol", OracleDbType.Varchar2, 50, dto.Rol.Trim());

            var idSalida = command.Parameters.Add(
                "p_id_usuario",
                OracleDbType.Int32,
                ParameterDirection.Output);

            await command.ExecuteNonQueryAsync(cancellationToken);

            idCreado = idSalida.GetInt32Value();
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            // ORA-00001: restricción única sobre CORREO o NOMBRE_USUARIO.
            throw new UsuarioDuplicadoException(
                "Ya existe un usuario con ese nombre de usuario o correo electrónico.",
                ex);
        }

        var creado = await ConsultarPorIdAsync(connection, idCreado, cancellationToken);

        return creado ?? throw new InvalidOperationException(
            "El usuario se insertó, pero no fue posible recuperar el registro generado.");
    }

    public async Task<bool> ActualizarAsync(
        int id,
        ActualizacionUsuarioDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var actual = await ConsultarPorIdAsync(connection, id, cancellationToken);
        if (actual is null)
        {
            return false;
        }

        // Si el cliente omite "rol" o "activo", se conservan los valores actuales.
        var rol = string.IsNullOrWhiteSpace(dto.Rol) ? actual.Rol : dto.Rol.Trim();
        var activo = dto.Activo ?? actual.Activo;

        ProtegerAdministrador(actual, rol, activo);

        // El SP conserva la contraseña actual cuando p_password llega NULL.
        string? passwordHash = string.IsNullOrWhiteSpace(dto.Password)
            ? null
            : PasswordHasher.Hash(dto.Password.Trim());

        int filasAfectadas;

        try
        {
            using var command = CrearProcedimiento("sp_actualizar_usuario", connection);

            AgregarEntrada(command, "p_id_usuario", OracleDbType.Int32, null, id);
            AgregarEntrada(command, "p_nombre_completo", OracleDbType.Varchar2, 150, dto.NombreCompleto.Trim());
            AgregarEntrada(command, "p_nombre_usuario", OracleDbType.Varchar2, 50, dto.NombreUsuario.Trim());
            AgregarEntrada(command, "p_correo", OracleDbType.Varchar2, 150, dto.Correo.Trim());
            AgregarEntrada(command, "p_password", OracleDbType.Varchar2, 255, passwordHash);
            AgregarEntrada(command, "p_rol", OracleDbType.Varchar2, 50, rol);
            AgregarEntrada(command, "p_activo", OracleDbType.Int32, null, activo ? 1 : 0);

            var filasSalida = command.Parameters.Add(
                "p_filas",
                OracleDbType.Int32,
                ParameterDirection.Output);

            await command.ExecuteNonQueryAsync(cancellationToken);

            filasAfectadas = filasSalida.GetInt32Value();
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            throw new UsuarioDuplicadoException(
                "Ya existe otro usuario con ese nombre de usuario o correo electrónico.",
                ex);
        }

        return filasAfectadas > 0;
    }

    public async Task<bool> DesactivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // La regla de negocio se valida antes de ejecutar el procedimiento para
        // que el Administrador no pueda quedar sin acceso por ninguna vía.
        var actual = await ConsultarPorIdAsync(connection, id, cancellationToken);

        if (actual is null)
        {
            return false;
        }

        ProtegerAdministrador(actual, actual.Rol, activo: false);

        return await EjecutarCambioEstadoAsync(
            connection,
            "sp_desactivar_usuario",
            id,
            cancellationToken);
    }

    public async Task<bool> ActivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await EjecutarCambioEstadoAsync(
            connection,
            "sp_activar_usuario",
            id,
            cancellationToken);
    }

    public async Task<IReadOnlyList<UsuarioSinOdontologoDto>> ObtenerSinOdontologoAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_usuarios_sin_odontologo", connection);
        var cursor = AgregarCursorSalida(command);

        await command.ExecuteNonQueryAsync(cancellationToken);

        if (cursor.Value is not OracleRefCursor refCursor)
        {
            return Array.Empty<UsuarioSinOdontologoDto>();
        }

        using var reader = refCursor.GetDataReader();
        var lista = new List<UsuarioSinOdontologoDto>();

        while (reader.Read())
        {
            var activoOrdinal = reader.GetOrdinal("ACTIVO");
            var activo = reader.IsDBNull(activoOrdinal) ? 1 : Convert.ToInt32(reader.GetValue(activoOrdinal));
            lista.Add(new UsuarioSinOdontologoDto
            {
                IdUsuario = LeerEntero(reader, "ID_USUARIO"),
                NombreCompleto = LeerTexto(reader, "NOMBRE_COMPLETO"),
                NombreUsuario = LeerTexto(reader, "NOMBRE_USUARIO"),
                Correo = LeerTexto(reader, "CORREO"),
                Rol = LeerTexto(reader, "ROL"),
                Activo = activo == 1
            });
        }

        return lista;
    }

    /// <summary>
    /// Regla de negocio: el usuario con rol Administrador no puede quedar
    /// desactivado ni perder su rol. Se aplica tanto en la baja lógica como en
    /// la actualización general para evitar que la protección se eluda.
    /// </summary>
    private static void ProtegerAdministrador(UsuarioDto actual, string nuevoRol, bool activo)
    {
        if (!RolesProtegidos.EsAdministrador(actual.Rol))
        {
            return;
        }

        if (!RolesProtegidos.EsAdministrador(nuevoRol))
        {
            throw new AdministradorProtegidoException(
                "El usuario Administrador está protegido: no es posible cambiarle el rol. Asigna otro usuario administrador antes de modificarlo.");
        }

        if (!activo)
        {
            throw new AdministradorProtegidoException(
                "El usuario Administrador está protegido y no puede ser desactivado ni eliminado. Esta regla evita dejar el sistema sin acceso administrativo.");
        }
    }

    private static async Task<bool> EjecutarCambioEstadoAsync(
        OracleConnection connection,
        string procedimiento,
        int id,
        CancellationToken cancellationToken)
    {
        using var command = CrearProcedimiento(procedimiento, connection);

        AgregarEntrada(command, "p_id_usuario", OracleDbType.Int32, null, id);

        var filasSalida = command.Parameters.Add(
            "p_filas",
            OracleDbType.Int32,
            ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return filasSalida.GetInt32Value() > 0;
    }

    private static async Task<UsuarioDto?> ConsultarPorIdAsync(
        OracleConnection connection,
        int id,
        CancellationToken cancellationToken)
    {
        using var command = CrearProcedimiento("sp_obtener_usuario", connection);

        AgregarEntrada(command, "p_id_usuario", OracleDbType.Int32, null, id);

        var cursor = AgregarCursorSalida(command);

        await command.ExecuteNonQueryAsync(cancellationToken);

        var usuarios = LeerUsuarios(cursor);
        return usuarios.Count > 0 ? usuarios[0] : null;
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

    private static OracleParameter AgregarCursorSalida(OracleCommand command)
    {
        return command.Parameters.Add(
            "p_cursor",
            OracleDbType.RefCursor,
            ParameterDirection.Output);
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

    private static IReadOnlyList<UsuarioDto> LeerUsuarios(OracleParameter cursor)
    {
        if (cursor.Value is not OracleRefCursor refCursor)
        {
            return Array.Empty<UsuarioDto>();
        }

        using var reader = refCursor.GetDataReader();

        var usuarios = new List<UsuarioDto>();

        while (reader.Read())
        {
            usuarios.Add(MapearUsuario(reader));
        }

        return usuarios;
    }

    private static UsuarioDto MapearUsuario(OracleDataReader reader)
    {
        var activoOrdinal = reader.GetOrdinal("ACTIVO");
        var activo = reader.IsDBNull(activoOrdinal)
            ? 1
            : Convert.ToInt32(reader.GetValue(activoOrdinal));

        return new UsuarioDto
        {
            IdUsuario = LeerEntero(reader, "ID_USUARIO"),
            NombreCompleto = LeerTexto(reader, "NOMBRE_COMPLETO"),
            NombreUsuario = LeerTexto(reader, "NOMBRE_USUARIO"),
            Correo = LeerTexto(reader, "CORREO"),
            Rol = LeerTexto(reader, "ROL"),
            Activo = activo == 1
        };
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
}
