using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<CategoriaDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
}