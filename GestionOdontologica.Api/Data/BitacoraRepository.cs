using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Data;

public sealed class BitacoraRepository : IBitacoraRepository
{
    private const int CommandTimeoutSeconds = 30;
    private readonly IOracleConnectionFactory _connectionFactory;

    public BitacoraRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<BitacoraPaginaDto> ConsultarAsync(BitacoraConsultaDto consulta, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_consultar_bitacoras", connection);
        command.Parameters.Add("p_desde", OracleDbType.TimeStamp, FechaONulo(consulta.Desde), ParameterDirection.Input);
        command.Parameters.Add("p_hasta", OracleDbType.TimeStamp, FechaONulo(consulta.Hasta), ParameterDirection.Input);
        command.Parameters.Add("p_modulo", OracleDbType.Varchar2, TextoONulo(consulta.Modulo), ParameterDirection.Input);
        command.Parameters.Add("p_resultado", OracleDbType.Varchar2, TextoONulo(consulta.Resultado), ParameterDirection.Input);
        command.Parameters.Add("p_texto", OracleDbType.Varchar2, TextoONulo(consulta.Texto), ParameterDirection.Input);
        command.Parameters.Add("p_pagina", OracleDbType.Int32, Math.Max(consulta.Pagina, 1), ParameterDirection.Input);
        command.Parameters.Add("p_tamano", OracleDbType.Int32, Math.Clamp(consulta.Tamano, 10, 100), ParameterDirection.Input);
        var totalParam = command.Parameters.Add("p_total", OracleDbType.Int32, ParameterDirection.Output);
        var cursor = command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        var items = new List<BitacoraDto>();
        using (var reader = ((OracleRefCursor)cursor.Value).GetDataReader())
        {
            while (reader.Read())
            {
                items.Add(new BitacoraDto
                {
                    IdBitacora = Convert.ToInt64(reader["ID_BITACORA"]),
                    FechaHora = Convert.ToDateTime(reader["FECHA_HORA"]),
                    Modulo = reader["MODULO_ETIQUETA"].ToString() ?? string.Empty,
                    Accion = reader["ACCION_ETIQUETA"].ToString() ?? string.Empty,
                    Entidad = reader["ENTIDAD_ETIQUETA"].ToString() ?? string.Empty,
                    Resultado = reader["RESULTADO_ETIQUETA"].ToString() ?? string.Empty,
                    ResultadoClave = reader["RESULTADO_CLAVE"].ToString() ?? string.Empty,
                    Usuario = reader["USUARIO"].ToString() ?? string.Empty,
                    DireccionIp = reader["DIRECCION_IP"] == DBNull.Value ? null : reader["DIRECCION_IP"].ToString(),
                    Detalle = reader["DETALLE"] == DBNull.Value ? null : reader["DETALLE"].ToString()
                });
            }
        }

        return new BitacoraPaginaDto
        {
            Total = totalParam.GetInt32Value(),
            Pagina = Math.Max(consulta.Pagina, 1),
            Tamano = Math.Clamp(consulta.Tamano, 10, 100),
            Items = items
        };
    }

    public async Task<BitacoraResumenDto> ObtenerResumenAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_bitacoras_resumen", connection);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new BitacoraResumenDto();
        }

        return new BitacoraResumenDto
        {
            TotalEventos = Convert.ToInt32(reader["TOTAL_EVENTOS"]),
            TotalErrores = Convert.ToInt32(reader["TOTAL_ERRORES"]),
            TotalAdvertencias = Convert.ToInt32(reader["TOTAL_ADVERTENCIAS"]),
            EventosHoy = Convert.ToInt32(reader["EVENTOS_HOY"]),
            UltimoEvento = reader["ULTIMO_EVENTO"] == DBNull.Value ? null : Convert.ToDateTime(reader["ULTIMO_EVENTO"])
        };
    }

    public async Task<BitacoraAccesoResumenDto> ObtenerResumenAccesosAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_accesos_resumen", connection);
        command.Parameters.Add("p_desde", OracleDbType.TimeStamp, FechaONulo(desde), ParameterDirection.Input);
        command.Parameters.Add("p_hasta", OracleDbType.TimeStamp, FechaONulo(hasta), ParameterDirection.Input);
        var cursor = command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new BitacoraAccesoResumenDto();
        }

        return new BitacoraAccesoResumenDto
        {
            TotalAccesos = Convert.ToInt32(reader["TOTAL_ACCESOS"]),
            AccesosExitosos = Convert.ToInt32(reader["ACCESOS_EXITOSOS"]),
            AccesosFallidos = Convert.ToInt32(reader["ACCESOS_FALLIDOS"]),
            AccesosHoy = Convert.ToInt32(reader["ACCESOS_HOY"]),
            DireccionesDistintas = Convert.ToInt32(reader["DIRECCIONES_DISTINTAS"]),
            UltimoAcceso = reader["ULTIMO_ACCESO"] == DBNull.Value ? null : Convert.ToDateTime(reader["ULTIMO_ACCESO"])
        };
    }

    public async Task<BitacoraFiltrosDto> ObtenerFiltrosAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_bitacoras_filtros", connection);
        var curModulos = command.Parameters.Add("p_modulos", OracleDbType.RefCursor, ParameterDirection.Output);
        var curResultados = command.Parameters.Add("p_resultados", OracleDbType.RefCursor, ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return new BitacoraFiltrosDto
        {
            Modulos = LeerFiltros(((OracleRefCursor)curModulos.Value).GetDataReader()),
            Resultados = LeerFiltros(((OracleRefCursor)curResultados.Value).GetDataReader())
        };
    }

    public async Task RegistrarExportacionAsync(int idUsuario, int registros, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_auditar_evento", connection);
        command.Parameters.Add("p_modulo", OracleDbType.Varchar2, "SISTEMA", ParameterDirection.Input);
        command.Parameters.Add("p_accion", OracleDbType.Varchar2, "EXPORTAR", ParameterDirection.Input);
        command.Parameters.Add("p_entidad", OracleDbType.Varchar2, "BITACORAS", ParameterDirection.Input);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
        command.Parameters.Add("p_id_registro", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_resultado", OracleDbType.Varchar2, "EXITO", ParameterDirection.Input);
        command.Parameters.Add("p_detalle", OracleDbType.Varchar2, $"Exportación de {registros} evento(s) de bitácora", ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RegistrarAccesoAsync(
        int? idUsuario,
        string accion,
        string resultado,
        string? direccionIp,
        string detalle,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_auditar_evento", connection);
        command.Parameters.Add("p_modulo", OracleDbType.Varchar2, "SESIONES", ParameterDirection.Input);
        command.Parameters.Add("p_accion", OracleDbType.Varchar2, accion, ParameterDirection.Input);
        command.Parameters.Add("p_entidad", OracleDbType.Varchar2, "SESIONES", ParameterDirection.Input);
        command.Parameters.Add("p_id_usuario", OracleDbType.Int32,
            idUsuario.HasValue ? idUsuario.Value : DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_id_registro", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input);
        command.Parameters.Add("p_resultado", OracleDbType.Varchar2, resultado, ParameterDirection.Input);
        command.Parameters.Add("p_detalle", OracleDbType.Varchar2, detalle, ParameterDirection.Input);
        command.Parameters.Add("p_direccion_ip", OracleDbType.Varchar2, TextoONulo(direccionIp), ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static List<BitacoraFiltroDto> LeerFiltros(OracleDataReader reader)
    {
        var lista = new List<BitacoraFiltroDto>();
        using (reader)
        {
            while (reader.Read())
            {
                lista.Add(new BitacoraFiltroDto
                {
                    Clave = reader["CLAVE"].ToString() ?? string.Empty,
                    Etiqueta = reader["ETIQUETA"].ToString() ?? string.Empty
                });
            }
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

    private static object FechaONulo(DateTime? fecha)
    {
        return fecha.HasValue ? fecha.Value : DBNull.Value;
    }

    private static object TextoONulo(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? DBNull.Value : texto.Trim();
    }
}
