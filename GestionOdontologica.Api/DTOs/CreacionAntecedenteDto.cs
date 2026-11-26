using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class CreacionAntecedenteDto
{
    [Required(ErrorMessage = "El tipo de antecedente es obligatorio.")]
    [RegularExpression("^(MEDICO|ODONTOLOGICO|ALERGICO|OTRO)$", ErrorMessage = "El tipo debe ser MEDICO, ODONTOLOGICO, ALERGICO u OTRO.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder los 1000 caracteres.")]
    public string? Observaciones { get; set; }
}
