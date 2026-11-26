namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Medicamento ya registrado en el expediente, usado como sugerencia para
/// nuevas indicaciones. No es un catalogo independiente.
/// </summary>
public sealed class MedicamentoCatalogoDto
{
    public string NombreMedicamento { get; init; } = string.Empty;
    public string? Dosis { get; init; }
    public string? Frecuencia { get; init; }
    public string? Duracion { get; init; }
    public string? Indicaciones { get; init; }
    public decimal Precio { get; init; }
}
