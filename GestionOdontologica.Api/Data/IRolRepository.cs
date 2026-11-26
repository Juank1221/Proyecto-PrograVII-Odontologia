using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IRolRepository
{
    Task<IReadOnlyList<RolDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<RolDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<RolDto> InsertarAsync(
        CreacionRolDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> ActualizarAsync(
        int id,
        ActualizacionRolDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DesactivarAsync(
        int id,
        CancellationToken cancellationToken = default);
}