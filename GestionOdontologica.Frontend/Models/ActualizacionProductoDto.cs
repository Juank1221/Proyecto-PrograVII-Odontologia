namespace GestionOdontologica.Frontend.Models;

public sealed class ActualizacionProductoDto
{
    public int IdCategoria { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Precio { get; set; }
    public DateTime? FechaCaducidad { get; set; }
    public bool? Activo { get; set; }
}
