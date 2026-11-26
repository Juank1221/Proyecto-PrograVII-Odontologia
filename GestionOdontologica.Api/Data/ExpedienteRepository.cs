using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Data;

public sealed class ExpedienteRepository : IExpedienteRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public ExpedienteRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ExpedienteDto?> ObtenerPorPacienteAsync(int idUsuario, int idPaciente, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_consultar_expediente", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        var curExpediente = command.Parameters.Add("p_cur_expediente", OracleDbType.RefCursor, ParameterDirection.Output);
        var curAtenciones = command.Parameters.Add("p_cur_atenciones", OracleDbType.RefCursor, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        ExpedienteDto? expediente = null;
        using (var reader = ((OracleRefCursor)curExpediente.Value).GetDataReader())
        {
            if (reader.Read())
            {
                expediente = new ExpedienteDto
                {
                    IdExpediente = Convert.ToInt32(reader["ID_EXPEDIENTE"]),
                    IdPaciente = Convert.ToInt32(reader["ID_PACIENTE"]),
                    FechaApertura = Convert.ToDateTime(reader["FECHA_APERTURA"]),
                    ObservacionesGenerales = LeerTexto(reader, "OBSERVACIONES_GENERALES"),
                    Estado = reader["ESTADO"].ToString() ?? string.Empty
                };
            }
        }

        if (expediente is null)
        {
            return null;
        }

        var atenciones = new List<AtencionDto>();
        using (var reader = ((OracleRefCursor)curAtenciones.Value).GetDataReader())
        {
            while (reader.Read())
            {
                atenciones.Add(new AtencionDto
                {
                    IdAtencion = Convert.ToInt32(reader["ID_ATENCION"]),
                    IdCita = Convert.ToInt32(reader["ID_CITA"]),
                    IdExpediente = Convert.ToInt32(reader["ID_EXPEDIENTE"]),
                    IdOdontologo = Convert.ToInt32(reader["ID_ODONTOLOGO"]),
                    NombreOdontologo = reader["NOMBRE_ODONTOLOGO"].ToString() ?? string.Empty,
                    FechaAtencion = Convert.ToDateTime(reader["FECHA_ATENCION"]),
                    Diagnostico = reader["DIAGNOSTICO"].ToString() ?? string.Empty,
                    Observaciones = LeerTexto(reader, "OBSERVACIONES"),
                    Resultado = LeerTexto(reader, "RESULTADO"),
                    Recomendaciones = LeerTexto(reader, "RECOMENDACIONES")
                });
            }
        }

        expediente.Atenciones = atenciones;
        return expediente;
    }

    public async Task ActualizarExpedienteAsync(int idUsuario, int idExpediente, ActualizacionExpedienteDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_actualizar_expediente", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_expediente", OracleDbType.Int32, idExpediente, ParameterDirection.Input);
        command.Parameters.Add("p_observaciones_generales", OracleDbType.Varchar2, ValorONulo(dto.ObservacionesGenerales), ParameterDirection.Input);
        command.Parameters.Add("p_estado", OracleDbType.Varchar2, dto.Estado.Trim(), ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> RegistrarAtencionAsync(int idUsuario, int idCita, CreacionAtencionDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_registrar_atencion", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_cita", OracleDbType.Int32, idCita, ParameterDirection.Input);
        command.Parameters.Add("p_diagnostico", OracleDbType.Varchar2, dto.Diagnostico.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_observaciones", OracleDbType.Varchar2, ValorONulo(dto.Observaciones), ParameterDirection.Input);
        command.Parameters.Add("p_resultado", OracleDbType.Varchar2, ValorONulo(dto.Resultado), ParameterDirection.Input);
        command.Parameters.Add("p_recomendaciones", OracleDbType.Varchar2, ValorONulo(dto.Recomendaciones), ParameterDirection.Input);
        var idParam = command.Parameters.Add("p_id_atencion", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return idParam.GetInt32Value();
    }

    public async Task ActualizarAtencionAsync(int idUsuario, int idAtencion, ActualizacionAtencionDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_actualizar_atencion", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_atencion", OracleDbType.Int32, idAtencion, ParameterDirection.Input);
        command.Parameters.Add("p_diagnostico", OracleDbType.Varchar2, dto.Diagnostico.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_observaciones", OracleDbType.Varchar2, ValorONulo(dto.Observaciones), ParameterDirection.Input);
        command.Parameters.Add("p_resultado", OracleDbType.Varchar2, ValorONulo(dto.Resultado), ParameterDirection.Input);
        command.Parameters.Add("p_recomendaciones", OracleDbType.Varchar2, ValorONulo(dto.Recomendaciones), ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<DetalleAtencionDto> ObtenerDetalleAtencionAsync(int idUsuario, int idAtencion, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_consultar_detalle_atencion", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_atencion", OracleDbType.Int32, idAtencion, ParameterDirection.Input);
        var curTratamientos = command.Parameters.Add("p_cur_tratamientos", OracleDbType.RefCursor, ParameterDirection.Output);
        var curMedicamentos = command.Parameters.Add("p_cur_medicamentos", OracleDbType.RefCursor, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        var tratamientos = new List<TratamientoAtencionDto>();
        using (var reader = ((OracleRefCursor)curTratamientos.Value).GetDataReader())
        {
            while (reader.Read())
            {
                tratamientos.Add(new TratamientoAtencionDto
                {
                    IdDetalleAtencion = Convert.ToInt32(reader["ID_DETALLE_ATENCION"]),
                    IdTratamiento = Convert.ToInt32(reader["ID_TRATAMIENTO"]),
                    NombreTratamiento = reader["NOMBRE_TRATAMIENTO"].ToString() ?? string.Empty,
                    Descripcion = LeerTexto(reader, "DESCRIPCION"),
                    PrecioAplicado = Convert.ToDecimal(reader["PRECIO_APLICADO"]),
                    Observaciones = LeerTexto(reader, "OBSERVACIONES")
                });
            }
        }

        var medicamentos = new List<MedicamentoIndicadoDto>();
        using (var reader = ((OracleRefCursor)curMedicamentos.Value).GetDataReader())
        {
            while (reader.Read())
            {
                medicamentos.Add(new MedicamentoIndicadoDto
                {
                    IdMedicamentoIndicado = Convert.ToInt32(reader["ID_MEDICAMENTO_INDICADO"]),
                    IdAtencion = Convert.ToInt32(reader["ID_ATENCION"]),
                    NombreMedicamento = reader["NOMBRE_MEDICAMENTO"].ToString() ?? string.Empty,
                    Dosis = LeerTexto(reader, "DOSIS"),
                    Frecuencia = LeerTexto(reader, "FRECUENCIA"),
                    Duracion = LeerTexto(reader, "DURACION"),
                    Indicaciones = LeerTexto(reader, "INDICACIONES"),
                    Precio = Convert.ToDecimal(reader["PRECIO"])
                });
            }
        }

        return new DetalleAtencionDto
        {
            IdAtencion = idAtencion,
            Tratamientos = tratamientos,
            Medicamentos = medicamentos
        };
    }

    public async Task<int> RegistrarTratamientoAsync(int idUsuario, int idAtencion, CreacionTratamientoAtencionDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_registrar_tratamiento_atencion", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_atencion", OracleDbType.Int32, idAtencion, ParameterDirection.Input);
        command.Parameters.Add("p_id_tratamiento", OracleDbType.Int32, dto.IdTratamiento, ParameterDirection.Input);
        command.Parameters.Add("p_descripcion", OracleDbType.Varchar2, ValorONulo(dto.Descripcion), ParameterDirection.Input);
        command.Parameters.Add("p_precio_aplicado", OracleDbType.Decimal, dto.PrecioAplicado.HasValue ? (object)dto.PrecioAplicado.Value : DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_observaciones", OracleDbType.Varchar2, ValorONulo(dto.Observaciones), ParameterDirection.Input);
        var idParam = command.Parameters.Add("p_id_detalle_atencion", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return idParam.GetInt32Value();
    }

    public async Task ActualizarTratamientoAsync(int idUsuario, int idDetalleAtencion, CreacionTratamientoAtencionDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_actualizar_detalle_atencion", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_detalle_atencion", OracleDbType.Int32, idDetalleAtencion, ParameterDirection.Input);
        command.Parameters.Add("p_id_tratamiento", OracleDbType.Int32, dto.IdTratamiento, ParameterDirection.Input);
        command.Parameters.Add("p_descripcion", OracleDbType.Varchar2, ValorONulo(dto.Descripcion), ParameterDirection.Input);
        command.Parameters.Add("p_precio_aplicado", OracleDbType.Decimal,
            dto.PrecioAplicado.HasValue ? (object)dto.PrecioAplicado.Value : DBNull.Value,
            ParameterDirection.Input);
        command.Parameters.Add("p_observaciones", OracleDbType.Varchar2, ValorONulo(dto.Observaciones), ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task EliminarTratamientoAsync(int idUsuario, int idDetalleAtencion, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_eliminar_detalle_atencion", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_detalle_atencion", OracleDbType.Int32, idDetalleAtencion, ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> RegistrarMedicamentoAsync(int idUsuario, int idAtencion, CreacionMedicamentoIndicadoDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_registrar_medicamento_indicado", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_atencion", OracleDbType.Int32, idAtencion, ParameterDirection.Input);
        command.Parameters.Add("p_nombre_medicamento", OracleDbType.Varchar2, dto.NombreMedicamento.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_dosis", OracleDbType.Varchar2, ValorONulo(dto.Dosis), ParameterDirection.Input);
        command.Parameters.Add("p_frecuencia", OracleDbType.Varchar2, ValorONulo(dto.Frecuencia), ParameterDirection.Input);
        command.Parameters.Add("p_duracion", OracleDbType.Varchar2, ValorONulo(dto.Duracion), ParameterDirection.Input);
        command.Parameters.Add("p_indicaciones", OracleDbType.Varchar2, ValorONulo(dto.Indicaciones), ParameterDirection.Input);
        command.Parameters.Add("p_precio", OracleDbType.Decimal,
            dto.Precio.HasValue ? (object)dto.Precio.Value : DBNull.Value,
            ParameterDirection.Input);
        var idParam = command.Parameters.Add("p_id_medicamento", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return idParam.GetInt32Value();
    }

    public async Task ActualizarMedicamentoAsync(int idUsuario, int idMedicamento, CreacionMedicamentoIndicadoDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_actualizar_medicamento_indicado", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_medicamento", OracleDbType.Int32, idMedicamento, ParameterDirection.Input);
        command.Parameters.Add("p_nombre_medicamento", OracleDbType.Varchar2, dto.NombreMedicamento.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_dosis", OracleDbType.Varchar2, ValorONulo(dto.Dosis), ParameterDirection.Input);
        command.Parameters.Add("p_frecuencia", OracleDbType.Varchar2, ValorONulo(dto.Frecuencia), ParameterDirection.Input);
        command.Parameters.Add("p_duracion", OracleDbType.Varchar2, ValorONulo(dto.Duracion), ParameterDirection.Input);
        command.Parameters.Add("p_indicaciones", OracleDbType.Varchar2, ValorONulo(dto.Indicaciones), ParameterDirection.Input);
        command.Parameters.Add("p_precio", OracleDbType.Decimal,
            dto.Precio.HasValue ? (object)dto.Precio.Value : DBNull.Value,
            ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task EliminarMedicamentoAsync(int idUsuario, int idMedicamento, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_eliminar_medicamento_indicado", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_medicamento", OracleDbType.Int32, idMedicamento, ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> RegistrarExamenAsync(int idUsuario, int idPaciente, CreacionExamenDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_registrar_examen", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        command.Parameters.Add("p_id_atencion", OracleDbType.Int32, dto.IdAtencion.HasValue ? (object)dto.IdAtencion.Value : DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_tipo_examen", OracleDbType.Varchar2, dto.TipoExamen.Trim(), ParameterDirection.Input);
        command.Parameters.Add("p_fecha_solicitud", OracleDbType.Date, FechaONulo(dto.FechaSolicitud), ParameterDirection.Input);
        command.Parameters.Add("p_fecha_realizacion", OracleDbType.Date, FechaONulo(dto.FechaRealizacion), ParameterDirection.Input);
        command.Parameters.Add("p_observaciones", OracleDbType.Varchar2, ValorONulo(dto.Observaciones), ParameterDirection.Input);
        var idParam = command.Parameters.Add("p_id_examen", OracleDbType.Int32, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return idParam.GetInt32Value();
    }

    public async Task ActualizarResultadoExamenAsync(int idUsuario, int idExamen, ActualizacionResultadoExamenDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_registrar_resultado_examen", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_examen", OracleDbType.Int32, idExamen, ParameterDirection.Input);
        command.Parameters.Add("p_fecha_realizacion", OracleDbType.Date, FechaONulo(dto.FechaRealizacion), ParameterDirection.Input);
        command.Parameters.Add("p_resultado", OracleDbType.Varchar2, ValorONulo(dto.Resultado), ParameterDirection.Input);
        command.Parameters.Add("p_observaciones", OracleDbType.Varchar2, ValorONulo(dto.Observaciones), ParameterDirection.Input);
        command.Parameters.Add("p_estado", OracleDbType.Varchar2, dto.Estado.Trim(), ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExamenDto>> ObtenerExamenesPacienteAsync(int idUsuario, int idPaciente, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_consultar_examenes_paciente", connection);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_paciente", OracleDbType.Int32, idPaciente, ParameterDirection.Input);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var lista = new List<ExamenDto>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new ExamenDto
            {
                IdExamen = Convert.ToInt32(reader["ID_EXAMEN"]),
                IdPaciente = Convert.ToInt32(reader["ID_PACIENTE"]),
                IdAtencion = reader["ID_ATENCION"] == DBNull.Value ? null : Convert.ToInt32(reader["ID_ATENCION"]),
                IdOdontologo = Convert.ToInt32(reader["ID_ODONTOLOGO"]),
                TipoExamen = reader["TIPO_EXAMEN"].ToString() ?? string.Empty,
                FechaSolicitud = Convert.ToDateTime(reader["FECHA_SOLICITUD"]),
                FechaRealizacion = reader["FECHA_REALIZACION"] == DBNull.Value ? null : Convert.ToDateTime(reader["FECHA_REALIZACION"]),
                Resultado = LeerTexto(reader, "RESULTADO"),
                Observaciones = LeerTexto(reader, "OBSERVACIONES"),
                Estado = reader["ESTADO"].ToString() ?? string.Empty
            });
        }

        return lista;
    }

    public async Task<IReadOnlyList<MedicamentoCatalogoDto>> ListarMedicamentosAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_medicamentos_indicados", connection);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var lista = new List<MedicamentoCatalogoDto>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new MedicamentoCatalogoDto
            {
                NombreMedicamento = reader["NOMBRE_MEDICAMENTO"].ToString() ?? string.Empty,
                Dosis = LeerTexto(reader, "DOSIS"),
                Frecuencia = LeerTexto(reader, "FRECUENCIA"),
                Duracion = LeerTexto(reader, "DURACION"),
                Indicaciones = LeerTexto(reader, "INDICACIONES"),
                Precio = Convert.ToDecimal(reader["PRECIO"])
            });
        }

        return lista;
    }

    private static OracleCommand CrearProcedimiento(string nombreProcedimiento, OracleConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = nombreProcedimiento;
        command.CommandTimeout = CommandTimeoutSeconds;
        return command;
    }

    private static string? LeerTexto(OracleDataReader reader, string columna)
    {
        return reader[columna] == DBNull.Value ? null : reader[columna].ToString();
    }

    private static object ValorONulo(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }

    private static object FechaONulo(DateTime? fecha)
    {
        return fecha.HasValue ? fecha.Value.Date : DBNull.Value;
    }
}
