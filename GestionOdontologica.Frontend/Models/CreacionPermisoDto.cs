using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

public sealed class CreacionPermisoDto
{
    [Required(ErrorMessage = "El nombre del permiso es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El módulo es obligatorio.")]
    [StringLength(50, ErrorMessage = "El módulo no puede superar los 50 caracteres.")]
    public string Modulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La acción es obligatoria.")]
    [StringLength(30, ErrorMessage = "La acción no puede superar los 30 caracteres.")]
    public string Accion { get; set; } = string.Empty;

    /// <summary>La tabla PERMISOS no almacena estado; se conserva por contrato.</summary>
    public bool Estado { get; set; } = true;

    /// <summary>La tabla PERMISOS no almacena código; se deriva de módulo y acción.</summary>
    [StringLength(80, ErrorMessage = "El código no puede superar los 80 caracteres.")]
    public string? Codigo { get; set; }
}
