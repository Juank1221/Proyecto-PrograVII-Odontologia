
namespace GestionOdontologica.Frontend.Models;

public sealed class RolDto
{
    public int IdRol { get; set; }

    public string NombreRol { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}
