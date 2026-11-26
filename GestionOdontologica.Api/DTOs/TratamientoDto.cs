namespace GestionOdontologica.Api.DTOs;

public sealed class TratamientoDto
{
    public int IdTratamiento { get; init; }
    public string CodigoTratamiento { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public decimal Precio { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime FechaModificacion { get; init; }
    public bool? AsociadoActivo { get; init; }
    public DateTime? FechaAsociacion { get; init; }
}

public sealed class AsociacionTratamientoDto
{
    public int IdTratamiento { get; set; }
}
