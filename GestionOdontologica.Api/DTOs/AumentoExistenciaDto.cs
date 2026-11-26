
using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class AumentoExistenciaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor que cero.")]
    public int IdProducto { get; set; }

    [Range(1L, 9999999999L, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public long Cantidad { get; set; }

    [Required(ErrorMessage = "El número de lote es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El número de lote debe tener entre 1 y 100 caracteres.")]
    public string NumeroLote { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El proveedor seleccionado no es válido.")]
    public int? IdProveedor { get; set; }

    public DateTime? FechaCaducidad { get; set; }

    [StringLength(255, ErrorMessage = "El motivo no puede superar los 255 caracteres.")]
    public string? Motivo { get; set; }
}
