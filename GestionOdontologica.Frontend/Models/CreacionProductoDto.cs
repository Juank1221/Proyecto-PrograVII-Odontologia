namespace GestionOdontologica.Frontend.Models;

public sealed class CreacionProductoDto
{
    public int IdCategoria { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public int StockMinimo { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public bool RequiereCaducidad { get; set; }
}
