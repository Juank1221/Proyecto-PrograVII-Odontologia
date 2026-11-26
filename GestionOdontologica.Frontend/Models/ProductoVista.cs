using System.Globalization;

namespace GestionOdontologica.Frontend.Models;

public sealed record ProductoVista(ProductoDto Producto)
{
    public bool Activo => Producto.Activo;
    public string ClaseFila => Activo ? string.Empty : "user-row--inactive";
    public string PrecioCostoTexto => $"₡{Producto.PrecioCosto.ToString("N2", CultureInfo.CurrentCulture)}";
    public string PrecioVentaTexto => $"₡{Producto.PrecioVenta.ToString("N2", CultureInfo.CurrentCulture)}";
}
