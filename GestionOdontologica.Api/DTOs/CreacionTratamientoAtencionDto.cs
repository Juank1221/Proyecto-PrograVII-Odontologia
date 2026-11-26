using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class CreacionTratamientoAtencionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El tratamiento es obligatorio.")]
    public int IdTratamiento { get; set; }

    [StringLength(255, ErrorMessage = "La descripcion no puede exceder los 255 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0, 999999.99, ErrorMessage = "El precio aplicado debe estar entre 0 y 999999.99.")]
    public decimal? PrecioAplicado { get; set; }

    [StringLength(255, ErrorMessage = "Las observaciones no pueden exceder los 255 caracteres.")]
    public string? Observaciones { get; set; }
}
