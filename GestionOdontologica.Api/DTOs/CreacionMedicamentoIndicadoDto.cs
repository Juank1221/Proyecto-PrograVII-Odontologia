using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class CreacionMedicamentoIndicadoDto
{
    [Required(ErrorMessage = "El nombre del medicamento es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
    public string NombreMedicamento { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "La dosis no puede exceder los 100 caracteres.")]
    public string? Dosis { get; set; }

    [StringLength(100, ErrorMessage = "La frecuencia no puede exceder los 100 caracteres.")]
    public string? Frecuencia { get; set; }

    [StringLength(100, ErrorMessage = "La duracion no puede exceder los 100 caracteres.")]
    public string? Duracion { get; set; }

    [StringLength(255, ErrorMessage = "Las indicaciones no pueden exceder los 255 caracteres.")]
    public string? Indicaciones { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal? Precio { get; set; }
}
