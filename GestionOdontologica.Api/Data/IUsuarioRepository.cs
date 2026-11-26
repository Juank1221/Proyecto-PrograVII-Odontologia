using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IUsuarioRepository
{
    Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<UsuarioDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UsuarioDto> InsertarAsync(CreacionUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<bool> ActualizarAsync(int id, ActualizacionUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<bool> DesactivarAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ActivarAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioSinOdontologoDto>> ObtenerSinOdontologoAsync(CancellationToken cancellationToken = default);
}
