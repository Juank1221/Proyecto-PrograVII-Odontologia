using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

/// <summary>
/// Toda la lógica de agenda se ejecuta en procedimientos almacenados: la API
/// solo valida la forma de la petición, los permisos y traduce los resultados.
/// </summary>
public sealed class CitaRepository : ICitaRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public CitaRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CitaDto>> ListarAsync(
        DateTime? fecha,
        int? idOdontologo,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_citas", connection);
        command.Parameters.Add("p_fecha", OracleDbType.Date,
            fecha.HasValue ? (object)fecha.Value.Date : DBNull.Value,
            ParameterDirection.Input);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32,
            idOdontologo.HasValue ? (object)idOdontologo.Value : DBNull.Value,
            ParameterDirection.Input);
        AgregarCursor(command);

        return await LeerListaAsync(command, MapearCita, cancellationToken);
    }

    public async Task<CitaDto?> ObtenerPorIdAsync(
        int idCita,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_obtener_cita", connection);
        command.Parameters.Add("p_id_cita", OracleDbType.Int32, idCita, ParameterDirection.Input);
        AgregarCursor(command);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapearCita(reader) : null;
    }

    public async Task<IReadOnlyList<DisponibilidadCitaDto>> ConsultarDisponibilidadAsync(
        DateTime fecha,
        int idOdontologo,
        int idTratamiento,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("SP_OBTENER_SLOTS_DISPONIBLES", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_fecha", OracleDbType.Date, fecha.Date, ParameterDirection.Input);
        command.Parameters.Add("p_id_tratamiento", OracleDbType.Int32, idTratamiento, ParameterDirection.Input);
        AgregarCursor(command);

        return await LeerListaAsync(command, reader =>
        {
            var estado = reader["ESTADO"].ToString() ?? string.Empty;
            return new DisponibilidadCitaDto
            {
                HoraInicio = reader["HORA_INICIO"].ToString() ?? string.Empty,
                HoraFin = reader["HORA_FIN"].ToString() ?? string.Empty,
                Estado = estado,
                Disponible = estado == "DISPONIBLE",
                Motivo = reader["MOTIVO"].ToString() ?? string.Empty
            };
        }, cancellationToken);
    }

    public async Task<ValidacionPacienteCitaDto?> ValidarPacienteAsync(
        int idPaciente,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_validar_paciente_cita", connection);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        AgregarCursor(command);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var campos = (reader["CAMPOS_FALTANTES"].ToString() ?? string.Empty)
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        return new ValidacionPacienteCitaDto
        {
            IdPaciente = Convert.ToInt32(reader["ID_PACIENTE"]),
            NombreCompleto = reader["NOMBRE_COMPLETO"].ToString() ?? string.Empty,
            Identificacion = reader["IDENTIFICACION"].ToString() ?? string.Empty,
            Telefono = reader["TELEFONO"].ToString() ?? string.Empty,
            Correo = reader["CORREO"].ToString() ?? string.Empty,
            Activo = Convert.ToInt32(reader["ACTIVO"]) == 1,
            CamposFaltantes = campos
        };
    }

    public async Task<int> AgendarAsync(
        CrearCitaRequest request,
        int idUsuarioRegistro,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("SP_REGISTRAR_CITA", connection);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, request.IdPaciente, ParameterDirection.Input);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, request.IdOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_id_tratamiento", OracleDbType.Int32, request.IdTratamiento, ParameterDirection.Input);
        command.Parameters.Add("p_fecha_hora_inicio", OracleDbType.TimeStamp,
            request.FechaHoraInicio, ParameterDirection.Input);
        command.Parameters.Add("p_motivo", OracleDbType.Varchar2,
            string.IsNullOrWhiteSpace(request.Motivo) ? DBNull.Value : (object)request.Motivo!.Trim(),
            ParameterDirection.Input);
        command.Parameters.Add("p_observaciones", OracleDbType.Varchar2,
            string.IsNullOrWhiteSpace(request.Observaciones) ? DBNull.Value : (object)request.Observaciones!.Trim(),
            ParameterDirection.Input);
        command.Parameters.Add("p_id_usuario_registro", OracleDbType.Int32, idUsuarioRegistro, ParameterDirection.Input);

        var parametroId = new OracleParameter("p_id_cita", OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(parametroId);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return Convert.ToInt32(parametroId.Value?.ToString());
    }

    public async Task<CitaDto?> MarcarLlegadaAsync(
        int idCita,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_marcar_llegada_cita", connection);
        command.Parameters.Add("p_id_cita", OracleDbType.Int32, idCita, ParameterDirection.Input);
        AgregarCursor(command);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapearCita(reader) : null;
    }

    public async Task<IReadOnlyList<NotificacionSalaEsperaDto>> ListarSalaEsperaAsync(
        int? idOdontologo,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_sala_espera", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32,
            idOdontologo.HasValue ? (object)idOdontologo.Value : DBNull.Value,
            ParameterDirection.Input);
        AgregarCursor(command);

        return await LeerListaAsync(command, reader => new NotificacionSalaEsperaDto
        {
            IdNotificacion = Convert.ToInt32(reader["ID_NOTIFICACION"]),
            IdCita = Convert.ToInt32(reader["ID_CITA"]),
            IdOdontologo = Convert.ToInt32(reader["ID_ODONTOLOGO"]),
            Mensaje = reader["MENSAJE"].ToString() ?? string.Empty,
            Leida = Convert.ToInt32(reader["LEIDA"]) == 1,
            FechaCreacion = Convert.ToDateTime(reader["FECHA_CREACION"]),
            FechaHoraInicio = Convert.ToDateTime(reader["FECHA_HORA_INICIO"]),
            NombrePaciente = reader["NOMBRE_PACIENTE"].ToString() ?? string.Empty
        }, cancellationToken);
    }

    public async Task<bool> MarcarNotificacionLeidaAsync(
        int idNotificacion,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_marcar_notificacion_leida", connection);
        command.Parameters.Add("p_id_notificacion", OracleDbType.Int32, idNotificacion, ParameterDirection.Input);

        var parametroFilas = new OracleParameter("p_filas", OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(parametroFilas);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return Convert.ToInt32(parametroFilas.Value?.ToString()) > 0;
    }

    public async Task<MiOdontologoDto?> ObtenerMiOdontologoAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_obtener_odontologo_usuario", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        AgregarCursor(command);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new MiOdontologoDto
        {
            IdOdontologo = Convert.ToInt32(reader["ID_ODONTOLOGO"]),
            NombreCompleto = reader["NOMBRE_COMPLETO"].ToString() ?? string.Empty
        };
    }

    public async Task<IReadOnlyList<CitaParaAtencionDto>> ListarParaAtencionAsync(
        int idUsuario,
        int? idPaciente,
        DateTime? fecha,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_citas_para_atencion", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32,
            idPaciente.HasValue ? (object)idPaciente.Value : DBNull.Value,
            ParameterDirection.Input);
        command.Parameters.Add("p_fecha", OracleDbType.Date,
            fecha.HasValue ? (object)fecha.Value.Date : DBNull.Value,
            ParameterDirection.Input);
        AgregarCursor(command);

        return await LeerListaAsync(command, reader => new CitaParaAtencionDto
        {
            IdCita = Convert.ToInt32(reader["ID_CITA"]),
            IdPaciente = Convert.ToInt32(reader["ID_PACIENTE"]),
            IdOdontologo = Convert.ToInt32(reader["ID_ODONTOLOGO"]),
            FechaHoraInicio = Convert.ToDateTime(reader["FECHA_HORA_INICIO"]),
            FechaHoraFin = Convert.ToDateTime(reader["FECHA_HORA_FIN"]),
            Estado = reader["ESTADO"].ToString() ?? string.Empty,
            Motivo = reader["MOTIVO"] == DBNull.Value ? null : reader["MOTIVO"].ToString(),
            DuracionMinutos = Convert.ToInt32(reader["DURACION_MINUTOS"]),
            NombrePaciente = reader["NOMBRE_PACIENTE"].ToString() ?? string.Empty,
            NombreTratamiento = reader["NOMBRE_TRATAMIENTO"] == DBNull.Value
                ? null : reader["NOMBRE_TRATAMIENTO"].ToString(),
            FechaHoraInicioAtencion = ObtenerFechaNullable(reader, "FECHA_HORA_INICIO_ATENCION")
        }, cancellationToken);
    }

    public Task<CitaDto?> IniciarAtencionAsync(
        int idUsuario,
        int idCita,
        CancellationToken cancellationToken = default) =>
        EjecutarTransicionAsync("sp_iniciar_atencion_cita", idUsuario, idCita, cancellationToken);

    public Task<CitaDto?> FinalizarAtencionAsync(
        int idUsuario,
        int idCita,
        CancellationToken cancellationToken = default) =>
        EjecutarTransicionAsync("sp_finalizar_atencion_cita", idUsuario, idCita, cancellationToken);

    private async Task<CitaDto?> EjecutarTransicionAsync(
        string procedimiento,
        int idUsuario,
        int idCita,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento(procedimiento, connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_cita", OracleDbType.Int32, idCita, ParameterDirection.Input);
        AgregarCursor(command);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapearCita(reader) : null;
    }

    private static async Task<IReadOnlyList<T>> LeerListaAsync<T>(
        OracleCommand command,
        Func<OracleDataReader, T> mapeador,
        CancellationToken cancellationToken)
    {
        var lista = new List<T>();

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(mapeador(reader));
        }

        return lista;
    }

    private static CitaDto MapearCita(OracleDataReader reader)
    {
        return new CitaDto
        {
            IdCita = Convert.ToInt32(reader["ID_CITA"]),
            IdPaciente = Convert.ToInt32(reader["ID_PACIENTE"]),
            IdOdontologo = Convert.ToInt32(reader["ID_ODONTOLOGO"]),
            IdTratamiento = ObtenerEnteroNullable(reader, "ID_TRATAMIENTO"),
            FechaHoraInicio = Convert.ToDateTime(reader["FECHA_HORA_INICIO"]),
            FechaHoraFin = Convert.ToDateTime(reader["FECHA_HORA_FIN"]),
            FechaHoraLlegada = ObtenerFechaNullable(reader, "FECHA_HORA_LLEGADA"),
            Motivo = reader["MOTIVO"] == DBNull.Value ? null : reader["MOTIVO"].ToString(),
            Estado = reader["ESTADO"].ToString() ?? string.Empty,
            Observaciones = reader["OBSERVACIONES"] == DBNull.Value ? null : reader["OBSERVACIONES"].ToString(),
            DuracionMinutos = Convert.ToInt32(reader["DURACION_MINUTOS"]),
            NombrePaciente = reader["NOMBRE_PACIENTE"].ToString() ?? string.Empty,
            IdentificacionPaciente = reader["IDENTIFICACION_PACIENTE"] == DBNull.Value
                ? null : reader["IDENTIFICACION_PACIENTE"].ToString(),
            TelefonoPaciente = reader["TELEFONO_PACIENTE"] == DBNull.Value
                ? null : reader["TELEFONO_PACIENTE"].ToString(),
            CorreoPaciente = reader["CORREO_PACIENTE"] == DBNull.Value
                ? null : reader["CORREO_PACIENTE"].ToString(),
            NombreOdontologo = reader["NOMBRE_ODONTOLOGO"].ToString() ?? string.Empty,
            NombreTratamiento = reader["NOMBRE_TRATAMIENTO"] == DBNull.Value
                ? null : reader["NOMBRE_TRATAMIENTO"].ToString()
        };
    }

    private static int? ObtenerEnteroNullable(OracleDataReader reader, string columna)
    {
        var indice = reader.GetOrdinal(columna);
        return reader.IsDBNull(indice) ? null : Convert.ToInt32(reader.GetValue(indice));
    }

    private static DateTime? ObtenerFechaNullable(OracleDataReader reader, string columna)
    {
        var indice = reader.GetOrdinal(columna);
        return reader.IsDBNull(indice) ? null : Convert.ToDateTime(reader.GetValue(indice));
    }

    private static void AgregarCursor(OracleCommand command)
    {
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
    }

    private static OracleCommand CrearProcedimiento(string nombre, OracleConnection connection)
    {
        return new OracleCommand(nombre, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = CommandTimeoutSeconds,
            BindByName = true
        };
    }
}
