using System.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Exceptions;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

public sealed class OdontologoRepository : IOdontologoRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public OdontologoRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<OdontologoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_odontologos", connection);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var lista = new List<OdontologoDto>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(MapearOdontologo(reader));
        }

        return lista;
    }

    public async Task<OdontologoDto?> ObtenerPorIdAsync(int idOdontologo, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_obtener_odontologo", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapearOdontologo(reader);
        }

        return null;
    }

    public async Task<OdontologoDto> InsertarAsync(CreacionOdontologoDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_insertar_odontologo", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, dto.IdUsuario.HasValue ? (object)dto.IdUsuario.Value : DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_nombres", OracleDbType.Varchar2, dto.Nombres.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_apellidos", OracleDbType.Varchar2, dto.Apellidos.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_numero_colegiado", OracleDbType.Varchar2, dto.NumeroColegiado.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_especialidad", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Especialidad) ? (object)DBNull.Value : dto.Especialidad.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_telefono", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Telefono) ? (object)DBNull.Value : dto.Telefono.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_correo", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Correo) ? (object)DBNull.Value : dto.Correo.Trim(), ParameterDirection.Input);
        var idParam = command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, ParameterDirection.Output);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            throw new OdontologoDuplicadoException("Ya existe un odontólogo con ese número de colegiado o correo electrónico.");
        }

        var idOdontologo = idParam.GetInt32Value();
        var odontologo = await ObtenerPorIdAsync(idOdontologo, cancellationToken);
        return odontologo ?? throw new InvalidOperationException("No fue posible recuperar el odontólogo recién creado.");
    }

    public async Task<OdontologoDto> InsertarVinculadoAsync(CreacionOdontologoVinculadoDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_crear_odontologo_vinculado", connection);
        command.Parameters.Add("p_accion", OracleDbType.Varchar2, dto.TipoVinculacion.Trim().ToUpperInvariant(), ParameterDirection.Input);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, dto.IdUsuario.HasValue ? (object)dto.IdUsuario.Value : DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_nombre_completo", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.NombreCompleto) ? (object)DBNull.Value : dto.NombreCompleto.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_nombre_usuario", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.NombreUsuario) ? (object)DBNull.Value : dto.NombreUsuario.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_correo_usuario", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.CorreoUsuario) ? (object)DBNull.Value : dto.CorreoUsuario.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_password_hash", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Password) ? (object)DBNull.Value : GestionOdontologica.Api.Security.PasswordHasher.Hash(dto.Password), ParameterDirection.Input);
        command.Parameters.Add("p_rol", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Rol) ? (object)"ODONTOLOGO" : dto.Rol.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_nombres", OracleDbType.Varchar2, dto.Nombres.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_apellidos", OracleDbType.Varchar2, dto.Apellidos.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_numero_colegiado", OracleDbType.Varchar2, dto.NumeroColegiado.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_especialidad", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Especialidad) ? (object)DBNull.Value : dto.Especialidad.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_telefono", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Telefono) ? (object)DBNull.Value : dto.Telefono.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_correo", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Correo) ? (object)DBNull.Value : dto.Correo.Trim(), ParameterDirection.Input);
        var idParam = command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, ParameterDirection.Output);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            throw new OdontologoDuplicadoException("Ya existe un odontólogo con ese número de colegiado o correo electrónico.");
        }
        catch (OracleException ex) when (ex.Number >= 20000 && ex.Number <= 20999)
        {
            throw new OdontologoDuplicadoException(ex.Message);
        }

        var idOdontologo = idParam.GetInt32Value();
        var odontologo = await ObtenerPorIdAsync(idOdontologo, cancellationToken);
        return odontologo ?? throw new InvalidOperationException("No fue posible recuperar el odontólogo recién creado.");
    }

    public async Task<OdontologoDto?> ActualizarAsync(int idOdontologo, ActualizacionOdontologoDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_actualizar_odontologo", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, dto.IdUsuario.HasValue ? (object)dto.IdUsuario.Value : DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_nombres", OracleDbType.Varchar2, dto.Nombres.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_apellidos", OracleDbType.Varchar2, dto.Apellidos.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_numero_colegiado", OracleDbType.Varchar2, dto.NumeroColegiado.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_especialidad", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Especialidad) ? (object)DBNull.Value : dto.Especialidad.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_telefono", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Telefono) ? (object)DBNull.Value : dto.Telefono.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_correo", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(dto.Correo) ? (object)DBNull.Value : dto.Correo.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_activo", OracleDbType.Int32, dto.Activo.HasValue ? (object)(dto.Activo.Value ? 1 : 0) : DBNull.Value, ParameterDirection.Input);
        var filasParam = command.Parameters.Add("p_filas", OracleDbType.Int32, ParameterDirection.Output);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            throw new OdontologoDuplicadoException("Ya existe un odontólogo con ese número de colegiado o correo electrónico.");
        }

        if (filasParam.GetInt32Value() == 0)
        {
            return null;
        }

        return await ObtenerPorIdAsync(idOdontologo, cancellationToken);
    }

    public async Task<bool> DesactivarAsync(int idOdontologo, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_desactivar_odontologo_sincronizar", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        var filasParam = command.Parameters.Add("p_filas", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return filasParam.GetInt32Value() > 0;
    }

    public async Task<bool> ActivarAsync(int idOdontologo, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_activar_odontologo_sincronizar", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
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

    private static OdontologoDto MapearOdontologo(OracleDataReader reader)
    {
        return new OdontologoDto
        {
            IdOdontologo = Convert.ToInt32(reader["ID_ODONTOLOGO"]),
            IdUsuario = reader["ID_USUARIO"] == DBNull.Value ? null : Convert.ToInt32(reader["ID_USUARIO"]),
            Nombres = reader["NOMBRES"].ToString() ?? string.Empty,
            Apellidos = reader["APELLIDOS"].ToString() ?? string.Empty,
            NumeroColegiado = reader["NUMERO_COLEGIADO"].ToString() ?? string.Empty,
            Especialidad = reader["ESPECIALIDAD"] == DBNull.Value ? null : reader["ESPECIALIDAD"].ToString(),
            Telefono = reader["TELEFONO"] == DBNull.Value ? null : reader["TELEFONO"].ToString(),
            Correo = reader["CORREO"] == DBNull.Value ? null : reader["CORREO"].ToString(),
            Activo = Convert.ToInt32(reader["ACTIVO"]) == 1,
            FechaCreacion = Convert.ToDateTime(reader["FECHA_CREACION"]),
            FechaModificacion = Convert.ToDateTime(reader["FECHA_MODIFICACION"])
        };
    }
}
