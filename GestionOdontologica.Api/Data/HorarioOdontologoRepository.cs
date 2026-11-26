using System.Data;
using System.Text.Json;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Exceptions;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

public sealed class HorarioOdontologoRepository : IHorarioOdontologoRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public HorarioOdontologoRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<HorarioOdontologoDto>> ObtenerPorOdontologoAsync(int idOdontologo, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = CrearProcedimiento("sp_listar_horarios_odontologo", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var lista = new List<HorarioOdontologoDto>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new HorarioOdontologoDto
            {
                IdHorario = Convert.ToInt32(reader["ID_HORARIO"]),
                IdOdontologo = Convert.ToInt32(reader["ID_ODONTOLOGO"]),
                DiaSemana = Convert.ToInt32(reader["DIA_SEMANA"]),
                HoraInicio = reader["HORA_INICIO"].ToString() ?? string.Empty,
                HoraFin = reader["HORA_FIN"].ToString() ?? string.Empty,
                Activo = Convert.ToInt32(reader["ACTIVO"]) == 1
            });
        }

        return lista;
    }

    public async Task ReemplazarHorariosAsync(int idOdontologo, ConfiguracionHorariosDto dto, CancellationToken cancellationToken = default)
    {
        dto.Horarios ??= new List<HorarioItemDto>();
        ValidarHorarios(dto);

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var items = dto.Horarios.Select(h => new
        {
            diaSemana = h.DiaSemana,
            horaInicio = h.HoraInicio,
            horaFin = h.HoraFin,
            activo = h.Activo ? 1 : 0
        });

        var json = JsonSerializer.Serialize(items);

        using var command = CrearProcedimiento("sp_reemplazar_horarios_odontologo", connection);
        command.Parameters.Add("p_id_odontologo", OracleDbType.Int32, idOdontologo, ParameterDirection.Input);
        command.Parameters.Add("p_horarios_json", OracleDbType.Clob, json, ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Valida los rangos antes de enviarlos a Oracle: cada turno activo debe
    /// tener una hora de fin posterior al inicio y no puede superponerse con
    /// otro turno activo del mismo día.
    /// </summary>
    private static void ValidarHorarios(ConfiguracionHorariosDto dto)
    {
        var activos = dto.Horarios.Where(h => h.Activo).ToList();
        if (activos.Count == 0)
        {
            return;
        }

        var rangos = new List<(int Dia, TimeSpan Inicio, TimeSpan Fin)>(activos.Count);

        foreach (var horario in activos)
        {
            if (horario.DiaSemana is < 1 or > 7)
            {
                throw new HorarioInvalidoException(
                    "El día de la semana debe estar entre 1 (lunes) y 7 (domingo).");
            }

            if (!TimeSpan.TryParse(horario.HoraInicio, out var inicio) ||
                !TimeSpan.TryParse(horario.HoraFin, out var fin))
            {
                throw new HorarioInvalidoException(
                    "Las horas de inicio y fin deben tener el formato HH:mm.");
            }

            if (inicio >= fin)
            {
                throw new HorarioInvalidoException(
                    $"En {NombreDia(horario.DiaSemana)}, la hora de inicio ({horario.HoraInicio}) debe ser anterior a la hora de fin ({horario.HoraFin}).");
            }

            rangos.Add((horario.DiaSemana, inicio, fin));
        }

        foreach (var grupo in rangos.GroupBy(r => r.Dia))
        {
            var ordenados = grupo.OrderBy(r => r.Inicio).ToList();

            for (var i = 1; i < ordenados.Count; i++)
            {
                var anterior = ordenados[i - 1];
                var actual = ordenados[i];

                if (actual.Inicio < anterior.Fin)
                {
                    var inicioTraslape = actual.Inicio;
                    var finTraslape = actual.Fin < anterior.Fin ? actual.Fin : anterior.Fin;

                    throw new HorarioInvalidoException(
                        $"Los horarios de {NombreDia(grupo.Key)} se traslapan entre {inicioTraslape:hh\\:mm} y {finTraslape:hh\\:mm}. Ajusta los turnos para que no se superpongan.");
                }
            }
        }
    }

    private static string NombreDia(int dia) => dia switch
    {
        1 => "lunes",
        2 => "martes",
        3 => "miércoles",
        4 => "jueves",
        5 => "viernes",
        6 => "sábado",
        7 => "domingo",
        _ => $"día {dia}"
    };

    private static OracleCommand CrearProcedimiento(string nombreProcedimiento, OracleConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = nombreProcedimiento;
        command.CommandTimeout = CommandTimeoutSeconds;
        return command;
    }
}
