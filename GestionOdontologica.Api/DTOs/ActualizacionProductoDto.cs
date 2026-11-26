
using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class ActualizacionProductoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
    public int IdCategoria { get; set; }

    [Required, StringLength(50, MinimumLength = 1)]
    public string Codigo { get; set; } = string.Empty;

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

    public bool? Activo { get; set; }
}
