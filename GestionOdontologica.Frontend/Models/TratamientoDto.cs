namespace GestionOdontologica.Frontend.Models;

public sealed class TratamientoDto
{
    public int IdTratamiento { get; set; }
    public string CodigoTratamiento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
    public bool? AsociadoActivo { get; set; }
    public DateTime? FechaAsociacion { get; set; }
}

public sealed class AsociacionTratamientoDto
{
    public int IdTratamiento { get; set; }
}
