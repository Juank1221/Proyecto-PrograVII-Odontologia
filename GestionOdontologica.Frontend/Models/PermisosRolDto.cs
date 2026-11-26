namespace GestionOdontologica.Frontend.Models;

public sealed class PermisosRolDto
{
    public int IdRol { get; set; }

    public string NombreRol { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public List<int> IdPermisos { get; set; } = new();
}
