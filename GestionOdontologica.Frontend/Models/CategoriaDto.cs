namespace GestionOdontologica.Frontend.Models;

public sealed class CategoriaDto
{
    public int IdCategoria { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
}