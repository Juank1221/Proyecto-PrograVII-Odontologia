using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

public sealed class ProductoFormModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
    public int IdCategoria { get; set; }

    [StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede superar los 255 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "La unidad de medida debe tener entre 1 y 30 caracteres.")]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int StockMinimo { get; set; }

    [Range(0, 999999999999.99, ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal PrecioCosto { get; set; }

    [Range(0, 999999999999.99, ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal PrecioVenta { get; set; }

    public bool RequiereCaducidad { get; set; }

    public bool EsEdicion { get; set; }
}
