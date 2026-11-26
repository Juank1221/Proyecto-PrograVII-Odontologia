namespace GestionOdontologica.Api.DTOs;

public sealed class ProductoDto
{
    public int IdProducto { get; init; }
    public int IdCategoria { get; init; }
    public string NombreCategoria { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string UnidadMedida { get; init; } = string.Empty;
    public int StockMinimo { get; init; }
    public decimal PrecioCosto { get; init; }
    public decimal PrecioVenta { get; init; }
    public bool RequiereCaducidad { get; init; }
    public bool Activo { get; init; }
}
