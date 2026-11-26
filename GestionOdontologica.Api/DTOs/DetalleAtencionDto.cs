namespace GestionOdontologica.Api.DTOs;

public sealed class DetalleAtencionDto
{
    public int IdAtencion { get; init; }
    public IReadOnlyList<TratamientoAtencionDto> Tratamientos { get; init; } = [];
    public IReadOnlyList<MedicamentoIndicadoDto> Medicamentos { get; init; } = [];
}

public sealed class TratamientoAtencionDto
{
    public int IdDetalleAtencion { get; init; }
    public int IdTratamiento { get; init; }
    public string NombreTratamiento { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public decimal PrecioAplicado { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class MedicamentoIndicadoDto
{
    public int IdMedicamentoIndicado { get; init; }
    public int IdAtencion { get; init; }
    public string NombreMedicamento { get; init; } = string.Empty;
    public string? Dosis { get; init; }
    public string? Frecuencia { get; init; }
    public string? Duracion { get; init; }
    public string? Indicaciones { get; init; }
    public decimal Precio { get; init; }
}
