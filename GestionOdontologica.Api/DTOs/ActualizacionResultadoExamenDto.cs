using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class ActualizacionResultadoExamenDto
{
    public DateTime? FechaRealizacion { get; set; }

    [StringLength(255, ErrorMessage = "El resultado no puede exceder los 255 caracteres.")]
    public string? Resultado { get; set; }

    [StringLength(255, ErrorMessage = "Las observaciones no pueden exceder los 255 caracteres.")]
    public string? Observaciones { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(Solicitado|En proceso|Realizado|Cancelado)$", ErrorMessage = "El estado debe ser Solicitado, En proceso, Realizado o Cancelado.")]
    public string Estado { get; set; } = string.Empty;
}
