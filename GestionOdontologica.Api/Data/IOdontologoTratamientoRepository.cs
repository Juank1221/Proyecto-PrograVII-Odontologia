using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IOdontologoTratamientoRepository
{
    Task<IReadOnlyList<TratamientoDto>> ObtenerPorOdontologoAsync(int idOdontologo, CancellationToken cancellationToken = default);
    Task AsociarAsync(int idOdontologo, int idTratamiento, CancellationToken cancellationToken = default);
    Task DesasociarAsync(int idOdontologo, int idTratamiento, CancellationToken cancellationToken = default);
}
