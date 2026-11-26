using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IPermisoRepository
{
    Task<IReadOnlyList<PermisoDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<PermisoDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PermisoDto> InsertarAsync(
        CreacionPermisoDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> ActualizarAsync(
        int id,
        ActualizacionPermisoDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ObtenerIdsPorRolAsync(
        int idRol,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ObtenerCodigosPorNombreRolAsync(
        string nombreRol,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ObtenerCodigosPorUsuarioAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task<bool> AsignarAsync(
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken = default);

    Task<bool> QuitarAsync(
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reemplaza las asignaciones del rol dentro de una unica transaccion.
    /// Solo ejecuta las altas y bajas necesarias (diff), no borra y recrea todo.
    /// </summary>
    Task<bool> ReemplazarAsignacionesAsync(
        int idRol,
        IReadOnlyCollection<int> idPermisos,
        CancellationToken cancellationToken = default);
}
