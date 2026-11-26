using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Services;

/// <summary>
/// Consulta los bloques de un día y, si el día está lleno, busca las próximas
/// fechas con al menos un bloque libre del mismo odontólogo y tratamiento.
/// Los días sin turno no tienen bloques, así que se descartan solos.
/// </summary>
public sealed class DisponibilidadCitasService
{
    private const int DiasSugeridos = 3;
    private const int MaximoDiasBusqueda = 30;

    private readonly ICitaRepository _citas;

    public DisponibilidadCitasService(ICitaRepository citas)
    {
        _citas = citas;
    }

    public async Task<DisponibilidadRespuestaDto> ObtenerAsync(
        int idOdontologo,
        DateTime fecha,
        int idTratamiento,
        CancellationToken cancellationToken = default)
    {
        var dia = fecha.Date;
        var bloques = await _citas.ConsultarDisponibilidadAsync(dia, idOdontologo, idTratamiento, cancellationToken);
        var hayDisponibilidad = bloques.Any(b => b.Disponible);
        var sugeridas = new List<DateTime>();

        if (!hayDisponibilidad)
        {
            var inicio = dia < DateTime.Today ? DateTime.Today : dia.AddDays(1);

            for (var i = 0; i < MaximoDiasBusqueda && sugeridas.Count < DiasSugeridos; i++)
            {
                var candidato = inicio.AddDays(i);
                var bloquesCandidato = await _citas.ConsultarDisponibilidadAsync(
                    candidato, idOdontologo, idTratamiento, cancellationToken);

                if (bloquesCandidato.Any(b => b.Disponible))
                {
                    sugeridas.Add(candidato);
                }
            }
        }

        return new DisponibilidadRespuestaDto
        {
            Fecha = dia,
            HayDisponibilidad = hayDisponibilidad,
            Bloques = bloques,
            ProximasFechasDisponibles = sugeridas
        };
    }
}
