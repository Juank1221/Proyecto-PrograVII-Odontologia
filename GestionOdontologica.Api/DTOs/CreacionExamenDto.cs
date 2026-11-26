using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class CreacionExamenDto
{
    public int? IdAtencion { get; set; }

    [Required(ErrorMessage = "El tipo de examen es obligatorio.")]
    [StringLength(100, ErrorMessage = "El tipo de examen no puede exceder los 100 caracteres.")]
    public string TipoExamen { get; set; } = string.Empty;

    public DateTime? FechaSolicitud { get; set; }

    public DateTime? FechaRealizacion { get; set; }

    [StringLength(255, ErrorMessage = "Las observaciones no pueden exceder los 255 caracteres.")]
    public string? Observaciones { get; set; }
}
