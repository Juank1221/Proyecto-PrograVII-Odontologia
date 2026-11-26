namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Conjunto de permisos que debe quedar asignado a un rol. El backend calcula
/// las altas y bajas y las aplica dentro de una transaccion.
/// </summary>
public sealed class ActualizacionPermisosRolDto
{
    public List<int> IdPermisos { get; set; } = new();
}
