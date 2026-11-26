namespace GestionOdontologica.Api.DTOs;

/// <summary>Rol con los identificadores de los permisos que tiene asignados.</summary>
public sealed class PermisosRolDto
{
    public int IdRol { get; set; }

    public string NombreRol { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public IReadOnlyList<int> IdPermisos { get; set; } = Array.Empty<int>();
}
