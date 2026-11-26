
using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface ITratamientoRepository
{
    Task<IReadOnlyList<TratamientoDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<TratamientoDto?> ObtenerPorIdAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default);

    Task<int> RegistrarAsync(
        string codigoTratamiento,
        string nombre,
        string? descripcion,
        decimal precio,
        CancellationToken cancellationToken = default);

    Task<bool> ActualizarAsync(
        int idTratamiento,
        string codigoTratamiento,
        string nombre,
        string? descripcion,
        decimal precio,
        CancellationToken cancellationToken = default);

    Task<bool> DesactivarAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default);

    Task<bool> ActivarAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default);
}
