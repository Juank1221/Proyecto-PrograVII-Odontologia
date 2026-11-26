
using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

public sealed class CreacionRolDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string NombreRol { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}
