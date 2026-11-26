
using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class AsignarProductosTratamientoRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Debe seleccionar al menos un producto.")]
    public List<ProductoTratamientoRequestDto> Productos { get; set; } = new();
}

public sealed class ProductoTratamientoRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto válido.")]
    public int IdProducto { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99",
        ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public decimal Cantidad { get; set; }
}
