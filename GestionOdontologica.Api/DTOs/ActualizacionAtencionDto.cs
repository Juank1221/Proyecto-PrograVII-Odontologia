using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class ActualizacionAtencionDto
{
    [Required(ErrorMessage = "El diagnostico es obligatorio.")]
    [StringLength(255, ErrorMessage = "El diagnostico no puede exceder los 255 caracteres.")]
    public string Diagnostico { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Las observaciones no pueden exceder los 255 caracteres.")]
    public string? Observaciones { get; set; }

    [StringLength(255, ErrorMessage = "El resultado no puede exceder los 255 caracteres.")]
    public string? Resultado { get; set; }

    [StringLength(255, ErrorMessage = "Las recomendaciones no pueden exceder los 255 caracteres.")]
    public string? Recomendaciones { get; set; }
}
