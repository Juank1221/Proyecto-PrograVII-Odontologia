
using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IProductoRepository
{
    Task<IReadOnlyList<ProductoDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<ProductoDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<string> ObtenerSiguienteCodigoAsync(
        CancellationToken cancellationToken = default);

    Task<ProductoDto> InsertarAsync(
        CreacionProductoDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> ActualizarAsync(
        int id,
        ActualizacionProductoDto dto,
        CancellationToken cancellationToken = default);

    Task<ProductoDto?> AumentarExistenciaAsync(
        int id,
        AumentoExistenciaDto dto,
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task<bool> DesactivarAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ActivarAsync(
        int id,
        CancellationToken cancellationToken = default);
}
