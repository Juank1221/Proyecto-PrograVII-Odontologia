
using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

public sealed class TratamientoRepository : ITratamientoRepository
{
    private const int CommandTimeoutSeconds = 60;

    private readonly IOracleConnectionFactory _connectionFactory;

    public TratamientoRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<TratamientoDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "sp_listar_tratamientos",
            connection);

        command.Parameters.Add(
            "p_cursor",
            OracleDbType.RefCursor,
            ParameterDirection.Output);

        var lista = new List<TratamientoDto>();

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(MapearTratamiento(reader));
        }

        return lista;
    }

    public async Task<TratamientoDto?> ObtenerPorIdAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "sp_obtener_tratamiento",
            connection);

        command.Parameters.Add(
            "p_id_tratamiento",
            OracleDbType.Int32,
            idTratamiento,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_cursor",
            OracleDbType.RefCursor,
            ParameterDirection.Output);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapearTratamiento(reader);
        }

        return null;
    }

    public async Task<int> RegistrarAsync(
        string codigoTratamiento,
        string nombre,
        string? descripcion,
        decimal precio,
        int duracionMinutos,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "sp_registrar_tratamiento",
            connection);

        command.Parameters.Add(
            "p_codigo_tratamiento",
            OracleDbType.Varchar2,
            codigoTratamiento,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_nombre",
            OracleDbType.Varchar2,
            nombre,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_descripcion",
            OracleDbType.Varchar2,
            string.IsNullOrWhiteSpace(descripcion)
                ? DBNull.Value
                : descripcion,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_precio",
            OracleDbType.Decimal,
            precio,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_duracion_minutos",
            OracleDbType.Int32,
            duracionMinutos,
            ParameterDirection.Input);

        var parametroId = new OracleParameter(
            "p_id_tratamiento",
            OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        command.Parameters.Add(parametroId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return Convert.ToInt32(parametroId.Value.ToString());
    }

    public async Task<bool> ActualizarAsync(
        int idTratamiento,
        string codigoTratamiento,
        string nombre,
        string? descripcion,
        decimal precio,
        int duracionMinutos,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "sp_actualizar_tratamiento",
            connection);

        command.Parameters.Add(
            "p_id_tratamiento",
            OracleDbType.Int32,
            idTratamiento,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_codigo_tratamiento",
            OracleDbType.Varchar2,
            codigoTratamiento,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_nombre",
            OracleDbType.Varchar2,
            nombre,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_descripcion",
            OracleDbType.Varchar2,
            string.IsNullOrWhiteSpace(descripcion)
                ? DBNull.Value
                : descripcion,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_precio",
            OracleDbType.Decimal,
            precio,
            ParameterDirection.Input);

        command.Parameters.Add(
            "p_duracion_minutos",
            OracleDbType.Int32,
            duracionMinutos,
            ParameterDirection.Input);

        var parametroFilas = new OracleParameter(
            "p_filas",
            OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        command.Parameters.Add(parametroFilas);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return Convert.ToInt32(parametroFilas.Value.ToString()) > 0;
    }

    public async Task<bool> DesactivarAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "sp_desactivar_tratamiento",
            connection);

        command.Parameters.Add(
            "p_id_tratamiento",
            OracleDbType.Int32,
            idTratamiento,
            ParameterDirection.Input);

        var parametroFilas = new OracleParameter(
            "p_filas",
            OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        command.Parameters.Add(parametroFilas);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return Convert.ToInt32(parametroFilas.Value.ToString()) > 0;
    }

    public async Task<bool> ActivarAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(
            "sp_activar_tratamiento",
            connection);

        command.Parameters.Add(
            "p_id_tratamiento",
            OracleDbType.Int32,
            idTratamiento,
            ParameterDirection.Input);

        var parametroFilas = new OracleParameter(
            "p_filas",
            OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        command.Parameters.Add(parametroFilas);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return Convert.ToInt32(parametroFilas.Value.ToString()) > 0;
    }

    private static TratamientoDto MapearTratamiento(OracleDataReader reader)
    {
        return new TratamientoDto
        {
            IdTratamiento = Convert.ToInt32(reader["ID_TRATAMIENTO"]),
            CodigoTratamiento = reader["CODIGO_TRATAMIENTO"].ToString()
                ?? string.Empty,
            Nombre = reader["NOMBRE"].ToString()
                ?? string.Empty,
            Descripcion = reader["DESCRIPCION"] == DBNull.Value
                ? null
                : reader["DESCRIPCION"].ToString(),
            Precio = Convert.ToDecimal(reader["PRECIO"]),
            DuracionMinutos = Convert.ToInt32(reader["DURACION_MINUTOS"]),
            Activo = Convert.ToInt32(reader["ACTIVO"]) == 1,
            FechaCreacion = Convert.ToDateTime(reader["FECHA_CREACION"]),
            FechaModificacion = Convert.ToDateTime(reader["FECHA_MODIFICACION"]),
            AsociadoActivo = ObtenerBooleanoNullable(reader, "ASOCIADO_ACTIVO"),
            FechaAsociacion = ObtenerFechaNullable(reader, "FECHA_ASOCIACION")
        };
    }

    private static bool? ObtenerBooleanoNullable(
        OracleDataReader reader,
        string columna)
    {
        try
        {
            var indice = reader.GetOrdinal(columna);

            if (reader.IsDBNull(indice))
                return null;

            return Convert.ToInt32(reader.GetValue(indice)) == 1;
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

    private static DateTime? ObtenerFechaNullable(
        OracleDataReader reader,
        string columna)
    {
        try
        {
            var indice = reader.GetOrdinal(columna);

            if (reader.IsDBNull(indice))
                return null;

            return Convert.ToDateTime(reader.GetValue(indice));
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

    private static OracleCommand CrearProcedimiento(
        string nombreProcedimiento,
        OracleConnection connection)
    {
        var command = connection.CreateCommand();

        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = nombreProcedimiento;
        command.CommandTimeout = CommandTimeoutSeconds;
        command.BindByName = true;

        return command;
    }
}
