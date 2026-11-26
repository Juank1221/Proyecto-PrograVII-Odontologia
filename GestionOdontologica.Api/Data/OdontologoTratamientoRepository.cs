using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

public sealed class OdontologoTratamientoRepository : IOdontologoTratamientoRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public OdontologoTratamientoRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<TratamientoDto>> ObtenerPorOdontologoAsync(int idOdontologo, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_tratamientos_odontologo", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var lista = new List<TratamientoDto>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new TratamientoDto
            {
                IdTratamiento = Convert.ToInt32(reader["ID_TRATAMIENTO"]),
                CodigoTratamiento = reader["CODIGO_TRATAMIENTO"].ToString() ?? string.Empty,
                Nombre = reader["NOMBRE"].ToString() ?? string.Empty,
                Descripcion = reader["DESCRIPCION"] == DBNull.Value ? null : reader["DESCRIPCION"].ToString(),
                Precio = Convert.ToDecimal(reader["PRECIO"]),
                Activo = Convert.ToInt32(reader["ACTIVO"]) == 1,
                AsociadoActivo = reader["ASOCIADO_ACTIVO"] == DBNull.Value ? null : Convert.ToInt32(reader["ASOCIADO_ACTIVO"]) == 1,
                FechaAsociacion = reader["FECHA_ASOCIACION"] == DBNull.Value ? null : Convert.ToDateTime(reader["FECHA_ASOCIACION"])
            });
        }

        return lista;
    }

    public async Task AsociarAsync(int idOdontologo, int idTratamiento, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_asociar_tratamiento_odontologo", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_id_tratamiento", OracleDbType.Int32, idTratamiento, ParameterDirection.Input);
        command.Parameters.Add("p_filas", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DesasociarAsync(int idOdontologo, int idTratamiento, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_desasociar_tratamiento_odontologo", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_id_tratamiento", OracleDbType.Int32, idTratamiento, ParameterDirection.Input);
        command.Parameters.Add("p_filas", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static OracleCommand CrearProcedimiento(string nombreProcedimiento, OracleConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = nombreProcedimiento;
        command.CommandTimeout = CommandTimeoutSeconds;
        return command;
    }
}
