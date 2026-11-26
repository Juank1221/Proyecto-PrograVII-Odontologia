using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class ActualizacionRolDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string NombreRol { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Debes indicar el estado del rol.")]
    public bool Activo { get; set; }
}