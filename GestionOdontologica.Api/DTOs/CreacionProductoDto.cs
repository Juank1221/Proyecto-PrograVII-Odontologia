
using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class CreacionProductoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
    public int IdCategoria { get; set; }

    [StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres.")]
    public string? Codigo { get; set; }

    [Required, StringLength(150, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Required, StringLength(50)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, 9999999999.99)]
    public decimal StockMinimo { get; set; }

    [Range(0, 999999999999.99)]
    public decimal PrecioCosto { get; set; }

    [Range(0, 999999999999.99)]
    public decimal PrecioVenta { get; set; }

    public bool RequiereCaducidad { get; set; }

    // Datos de la existencia inicial.
    [Range(0, 9999999999.99)]
    public decimal CantidadInicial { get; set; }

    public string? NumeroLote { get; set; }

    public int? IdProveedor { get; set; }

    public DateTime? FechaCaducidad { get; set; }
}
