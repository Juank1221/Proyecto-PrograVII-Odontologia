using System.Data;
using System.Text.Json;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Exceptions;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Data;

public sealed class PacienteRepository : IPacienteRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public PacienteRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PacienteDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_pacientes", connection);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var lista = new List<PacienteDto>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(MapearPaciente(reader));
        }

        return lista;
    }

    public async Task<PacienteDto?> ObtenerPorIdAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_obtener_paciente", connection);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapearPaciente(reader);
        }

        return null;
    }

    public async Task<PacienteDto> InsertarAsync(CreacionPacienteDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_insertar_paciente", connection);
        command.Parameters.Add("p_nombres", OracleDbType.Varchar2, dto.Nombres.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_apellidos", OracleDbType.Varchar2, dto.Apellidos.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_identificacion", OracleDbType.Varchar2, dto.Identificacion.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_fecha_nacimiento", OracleDbType.Date, dto.FechaNacimiento, ParameterDirection.Input);
        command.Parameters.Add("p_sexo", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Sexo) ? (object)DBNull.Value : dto.Sexo.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_telefono", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Telefono) ? (object)DBNull.Value : dto.Telefono.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_correo", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Correo) ? (object)DBNull.Value : dto.Correo.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_direccion", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Direccion) ? (object)DBNull.Value : dto.Direccion.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_contacto_emergencia_nombre", OracleDbType.Varchar2, ValorONulo(dto.ContactoEmergenciaNombre), ParameterDirection.Input);
        command.Parameters.Add("p_contacto_emergencia_telefono", OracleDbType.Varchar2, ValorONulo(dto.ContactoEmergenciaTelefono), ParameterDirection.Input);
        command.Parameters.Add("p_contacto_emergencia_parentesco", OracleDbType.Varchar2, ValorONulo(dto.ContactoEmergenciaParentesco), ParameterDirection.Input);
        var idParam = command.Parameters.Add("p_id_paciente", OracleDbType.Int32, ParameterDirection.Output);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            throw new PacienteDuplicadoException("Ya existe un paciente con esa identificación o correo electrónico.");
        }

        var idPaciente = idParam.GetInt32Value();
        var paciente = await ObtenerPorIdAsync(idPaciente, cancellationToken);
        return paciente ?? throw new InvalidOperationException("No fue posible recuperar el paciente recién creado.");
    }

    public async Task<PacienteDto?> ActualizarAsync(int idPaciente, ActualizacionPacienteDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_actualizar_paciente", connection);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        command.Parameters.Add("p_nombres", OracleDbType.Varchar2, dto.Nombres.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_apellidos", OracleDbType.Varchar2, dto.Apellidos.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_identificacion", OracleDbType.Varchar2, dto.Identificacion.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_fecha_nacimiento", OracleDbType.Date, dto.FechaNacimiento, ParameterDirection.Input);
        command.Parameters.Add("p_sexo", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Sexo) ? (object)DBNull.Value : dto.Sexo.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_telefono", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Telefono) ? (object)DBNull.Value : dto.Telefono.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_correo", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Correo) ? (object)DBNull.Value : dto.Correo.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_direccion", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Direccion) ? (object)DBNull.Value : dto.Direccion.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_activo", OracleDbType.Int32, dto.Activo.HasValue ? (object)(dto.Activo.Value ? 1 : 0) : DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_contacto_emergencia_nombre", OracleDbType.Varchar2, ValorONulo(dto.ContactoEmergenciaNombre), ParameterDirection.Input);
        command.Parameters.Add("p_contacto_emergencia_telefono", OracleDbType.Varchar2, ValorONulo(dto.ContactoEmergenciaTelefono), ParameterDirection.Input);
        command.Parameters.Add("p_contacto_emergencia_parentesco", OracleDbType.Varchar2, ValorONulo(dto.ContactoEmergenciaParentesco), ParameterDirection.Input);
        var filasParam = command.Parameters.Add("p_filas", OracleDbType.Int32, ParameterDirection.Output);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            throw new PacienteDuplicadoException("Ya existe un paciente con esa identificación o correo electrónico.");
        }

        if (filasParam.GetInt32Value() == 0)
        {
            return null;
        }

        return await ObtenerPorIdAsync(idPaciente, cancellationToken);
    }

    public async Task<bool> DesactivarAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_desactivar_paciente", connection);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        var filasParam = command.Parameters.Add("p_filas", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return filasParam.GetInt32Value() > 0;
    }

    public async Task<bool> ActivarAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_activar_paciente", connection);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        var filasParam = command.Parameters.Add("p_filas", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return filasParam.GetInt32Value() > 0;
    }

    private static OracleCommand CrearProcedimiento(string nombreProcedimiento, OracleConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = nombreProcedimiento;
        command.CommandTimeout = CommandTimeoutSeconds;
        return command;
    }

    private static object ValorONulo(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }

    private static PacienteDto MapearPaciente(OracleDataReader reader)
    {
        return new PacienteDto
        {
            IdPaciente = Convert.ToInt32(reader["ID_PACIENTE"]),
            Nombres = reader["NOMBRES"].ToString() ?? string.Empty,
            Apellidos = reader["APELLIDOS"].ToString() ?? string.Empty,
            Identificacion = reader["IDENTIFICACION"].ToString() ?? string.Empty,
            FechaNacimiento = reader["FECHA_NACIMIENTO"] == DBNull.Value ? null : Convert.ToDateTime(reader["FECHA_NACIMIENTO"]),
            Sexo = reader["SEXO"] == DBNull.Value ? null : reader["SEXO"].ToString(),
            Telefono = reader["TELEFONO"] == DBNull.Value ? null : reader["TELEFONO"].ToString(),
            Correo = reader["CORREO"] == DBNull.Value ? null : reader["CORREO"].ToString(),
            Direccion = reader["DIRECCION"] == DBNull.Value ? null : reader["DIRECCION"].ToString(),
            ContactoEmergenciaNombre = reader["CONTACTO_EMERGENCIA_NOMBRE"] == DBNull.Value ? null : reader["CONTACTO_EMERGENCIA_NOMBRE"].ToString(),
            ContactoEmergenciaTelefono = reader["CONTACTO_EMERGENCIA_TELEFONO"] == DBNull.Value ? null : reader["CONTACTO_EMERGENCIA_TELEFONO"].ToString(),
            ContactoEmergenciaParentesco = reader["CONTACTO_EMERGENCIA_PARENTESCO"] == DBNull.Value ? null : reader["CONTACTO_EMERGENCIA_PARENTESCO"].ToString(),
            Activo = Convert.ToInt32(reader["ACTIVO"]) == 1,
            FechaCreacion = Convert.ToDateTime(reader["FECHA_CREACION"]),
            FechaModificacion = Convert.ToDateTime(reader["FECHA_MODIFICACION"])
        };
    }
}
