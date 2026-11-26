using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

public sealed class AntecedenteRepository : IAntecedenteRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public AntecedenteRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AntecedenteDto>> ObtenerPorPacienteAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_antecedentes_paciente", connection);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var lista = new List<AntecedenteDto>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new AntecedenteDto
            {
                IdAntecedente = Convert.ToInt32(reader["ID_ANTECEDENTE"]),
                IdPaciente = Convert.ToInt32(reader["ID_PACIENTE"]),
                Tipo = reader["TIPO"].ToString() ?? string.Empty,
                Descripcion = reader["DESCRIPCION"].ToString() ?? string.Empty,
                Observaciones = reader["OBSERVACIONES"] == DBNull.Value ? null : reader["OBSERVACIONES"].ToString(),
                FechaRegistro = Convert.ToDateTime(reader["FECHA_REGISTRO"])
            });
        }

        return lista;
    }

    public async Task<AntecedenteDto> InsertarAsync(int idPaciente, CreacionAntecedenteDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_insertar_antecedente", connection);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        command.Parameters.Add("p_tipo", OracleDbType.Varchar2, dto.Tipo.Trim().ToUpperInvariant(), ParameterDirection.Input);
        command.Parameters.Add("p_descripcion", OracleDbType.Varchar2, dto.Descripcion.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_observaciones", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Observaciones) ? (object)DBNull.Value : dto.Observaciones.Trim(), ParameterDirection.Input);
        var idParam = command.Parameters.Add("p_id_antecedente", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
        var idAntecedente = idParam.GetInt32Value();

        var lista = await ObtenerPorPacienteAsync(idPaciente, cancellationToken);
        var antecedente = lista.FirstOrDefault(a => a.IdAntecedente == idAntecedente);
        if (antecedente != null) return antecedente;

        return new AntecedenteDto
        {
            IdAntecedente = idAntecedente,
            IdPaciente = idPaciente,
            Tipo = dto.Tipo.Trim().ToUpperInvariant(),
            Descripcion = dto.Descripcion.Trim(),
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim(),
            FechaRegistro = DateTime.Now
        };
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
