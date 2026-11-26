using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IOdontologoRepository
{
    Task<IReadOnlyList<OdontologoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<OdontologoDto?> ObtenerPorIdAsync(int idOdontologo, CancellationToken cancellationToken = default);
    Task<OdontologoDto> InsertarAsync(CreacionOdontologoDto dto, CancellationToken cancellationToken = default);
    Task<OdontologoDto> InsertarVinculadoAsync(CreacionOdontologoVinculadoDto dto, CancellationToken cancellationToken = default);
    Task<OdontologoDto?> ActualizarAsync(int idOdontologo, ActualizacionOdontologoDto dto, CancellationToken cancellationToken = default);
    Task<bool> DesactivarAsync(int idOdontologo, CancellationToken cancellationToken = default);
    Task<bool> ActivarAsync(int idOdontologo, CancellationToken cancellationToken = default);
}
