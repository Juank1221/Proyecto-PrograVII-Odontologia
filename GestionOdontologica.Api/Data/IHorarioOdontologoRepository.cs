using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IHorarioOdontologoRepository
{
    Task<IReadOnlyList<HorarioOdontologoDto>> ObtenerPorOdontologoAsync(int idOdontologo, CancellationToken cancellationToken = default);
    Task ReemplazarHorariosAsync(int idOdontologo, ConfiguracionHorariosDto dto, CancellationToken cancellationToken = default);
}
