namespace GestionOdontologica.Frontend.Models;

/// <summary>Permiso administrable devuelto por GET /api/permisos.</summary>
public sealed class PermisoDto
{
    public int IdPermiso { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Modulo { get; set; } = string.Empty;

    public string Accion { get; set; } = string.Empty;

    public bool Estado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int Asignaciones { get; set; }
}
