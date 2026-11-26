using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

public sealed class ProductoFormModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede superar los 255 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0, 9999999999.99, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public decimal StockMinimo { get; set; }

    [Range(0, 9999999999.99, ErrorMessage = "La cantidad no puede ser negativa.")]
    public decimal Cantidad { get; set; }

    [Range(0, 999999999999.99, ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal Precio { get; set; }

    public DateTime? FechaCaducidad { get; set; }

    public bool EsEdicion { get; set; }
}
