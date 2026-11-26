
namespace GestionOdontologica.Frontend.Models;

public sealed class AsignarProductosTratamientoRequest
{
    public List<ProductoTratamientoRequestDto> Productos { get; set; } = new();
}

public sealed class ProductoTratamientoRequestDto
{
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
}
