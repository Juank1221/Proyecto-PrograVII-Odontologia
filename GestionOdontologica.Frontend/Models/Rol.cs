using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.Models;

public class Rol
{
    public int IdRol { get; set; }

    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string NombreRol { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0, 1, ErrorMessage = "El estado debe ser 0 o 1.")]
    public int Activo { get; set; } = 1;

    public DateTime FechaCreacion { get; set; }
}