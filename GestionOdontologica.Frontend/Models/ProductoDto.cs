namespace GestionOdontologica.Frontend.Models;

public sealed class ProductoDto
{
    public int IdProducto { get; init; }
    public int IdCategoria { get; init; }
    public string NombreCategoria { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public decimal StockMinimo { get; init; }
    public decimal Cantidad { get; init; }
    public decimal Precio { get; init; }
    public DateTime? FechaCaducidad { get; init; }
    public bool Activo { get; init; }
}
