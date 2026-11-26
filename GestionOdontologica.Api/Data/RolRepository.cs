using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Data;

public sealed class RolRepository : IRolRepository
{
    private const int CommandTimeoutSeconds = 60;

    private readonly IOracleConnectionFactory _connectionFactory;

    public RolRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<RolDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "SP_LISTAR_ROLES",
            connection);

        var cursor = AgregarCursorSalida(command);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return LeerRoles(cursor);
    }

    public async Task<RolDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await ConsultarPorIdAsync(
            connection,
            id,
            cancellationToken);
    }

    public async Task<RolDto> InsertarAsync(
        CreacionRolDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        int idCreado;

        using (var command = CrearProcedimiento(
            "SP_INSERTAR_ROL",
            connection))
        {
            AgregarEntrada(
                command,
                "p_nombre_rol",
                OracleDbType.Varchar2,
                50,
                dto.NombreRol.Trim());

            AgregarEntrada(
                command,
                "p_descripcion",
                OracleDbType.Varchar2,
                250,
                string.IsNullOrWhiteSpace(dto.Descripcion)
                    ? null
                    : dto.Descripcion.Trim());

            AgregarEntrada(
                command,
                "p_activo",
                OracleDbType.Int32,
                null,
                dto.Activo ? 1 : 0);

            var idSalida = command.Parameters.Add(
                "p_id_rol",
                OracleDbType.Int32,
                ParameterDirection.Output);

            await command.ExecuteNonQueryAsync(cancellationToken);

            idCreado = idSalida.GetInt32Value();
        }

        var creado = await ConsultarPorIdAsync(
            connection,
            idCreado,
            cancellationToken);

        return creado ?? throw new InvalidOperationException(
            "El rol se insertó, pero no fue posible recuperar el registro.");
    }

    public async Task<bool> ActualizarAsync(
        int id,
        ActualizacionRolDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "SP_ACTUALIZAR_ROL",
            connection);

        AgregarEntrada(
            command,
            "p_id_rol",
            OracleDbType.Int32,
            null,
            id);

        AgregarEntrada(
            command,
            "p_nombre_rol",
            OracleDbType.Varchar2,
            50,
            dto.NombreRol.Trim());

        AgregarEntrada(
            command,
            "p_descripcion",
            OracleDbType.Varchar2,
            250,
            string.IsNullOrWhiteSpace(dto.Descripcion)
                ? null
                : dto.Descripcion.Trim());

        AgregarEntrada(
            command,
            "p_activo",
            OracleDbType.Int32,
            null,
            dto.Activo ? 1 : 0);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DesactivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "SP_ELIMINAR_ROL",
            connection);

        AgregarEntrada(
            command,
            "p_id_rol",
            OracleDbType.Int32,
            null,
            id);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return true;
    }

    private static async Task<RolDto?> ConsultarPorIdAsync(
        OracleConnection connection,
        int id,
        CancellationToken cancellationToken)
    {
        using var command = CrearProcedimiento(
            "SP_CONSULTAR_ROL",
            connection);

        AgregarEntrada(
            command,
            "p_id_rol",
            OracleDbType.Int32,
            null,
            id);

        var cursor = AgregarCursorSalida(command);

        await command.ExecuteNonQueryAsync(cancellationToken);

        var roles = LeerRoles(cursor);

        return roles.Count > 0 ? roles[0] : null;
    }

    private static OracleCommand CrearProcedimiento(
        string nombre,
        OracleConnection connection)
    {
        return new OracleCommand(nombre, connection)
        {
            CommandType = CommandType.StoredProcedure,
            BindByName = true,
            CommandTimeout = CommandTimeoutSeconds
        };
    }

    private static OracleParameter AgregarCursorSalida(
        OracleCommand command)
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

    private static IReadOnlyList<RolDto> LeerRoles(
        OracleParameter cursor)
    {
        if (cursor.Value is not OracleRefCursor refCursor)
        {
            return Array.Empty<RolDto>();
        }

        using var reader = refCursor.GetDataReader();

        var roles = new List<RolDto>();

        while (reader.Read())
        {
            roles.Add(MapearRol(reader));
        }

        return roles;
    }

    private static RolDto MapearRol(OracleDataReader reader)
    {
        var activoOrdinal = reader.GetOrdinal("ACTIVO");

        var activo = reader.IsDBNull(activoOrdinal)
            ? 1
            : Convert.ToInt32(reader.GetValue(activoOrdinal));

        var fechaOrdinal = reader.GetOrdinal("FECHA_CREACION");

        return new RolDto
        {
            IdRol = LeerEntero(reader, "ID_ROL"),
            NombreRol = LeerTexto(reader, "NOMBRE_ROL"),
            Descripcion = LeerTextoOpcional(reader, "DESCRIPCION"),
            Activo = activo == 1,
            FechaCreacion = reader.IsDBNull(fechaOrdinal)
                ? DateTime.MinValue
                : reader.GetDateTime(fechaOrdinal)
        };
    }

    private static string LeerTexto(
        OracleDataReader reader,
        string columna)
    {
        var ordinal = reader.GetOrdinal(columna);

        return reader.IsDBNull(ordinal)
            ? string.Empty
            : reader.GetString(ordinal);
    }

    private static string? LeerTextoOpcional(
        OracleDataReader reader,
        string columna)
    {
        var ordinal = reader.GetOrdinal(columna);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }

    private static int LeerEntero(
        OracleDataReader reader,
        string columna)
    {
        var ordinal = reader.GetOrdinal(columna);

        return reader.IsDBNull(ordinal)
            ? 0
            : Convert.ToInt32(reader.GetValue(ordinal));
    }
}