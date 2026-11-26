using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class ActualizacionExpedienteDto
{
    [StringLength(255, ErrorMessage = "Las observaciones generales no pueden exceder los 255 caracteres.")]
    public string? ObservacionesGenerales { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(Abierto|Cerrado|Suspendido)$", ErrorMessage = "El estado debe ser Abierto, Cerrado o Suspendido.")]
    public string Estado { get; set; } = string.Empty;
}
