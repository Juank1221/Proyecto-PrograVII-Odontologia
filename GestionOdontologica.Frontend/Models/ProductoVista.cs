using System.Globalization;

namespace GestionOdontologica.Frontend.Models;

public sealed record ProductoVista(ProductoDto Producto)
{
    private static readonly CultureInfo CulturaColones = CultureInfo.GetCultureInfo("es-CR");

    public bool Activo => Producto.Activo;
    public string ClaseFila => Activo ? string.Empty : "user-row--inactive";
    public string PrecioTexto => "₡" + Producto.Precio.ToString("N2", CulturaColones);
    public string CantidadTexto => Producto.Cantidad.ToString("0.##", CultureInfo.CurrentCulture);
    public string CaducidadTexto => Producto.FechaCaducidad?.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture) ?? "No aplica";
}
