
namespace GestionOdontologica.Api.DTOs;

public sealed class ProductoTratamientoDto
{
    public int IdTratamientoProducto { get; set; }
    public int IdTratamiento { get; set; }
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public bool Activo { get; set; }
}
