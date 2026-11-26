namespace GestionOdontologica.Frontend.Models;

public sealed class ExpedienteDto
{
    public int IdExpediente { get; set; }
    public int IdPaciente { get; set; }
    public DateTime FechaApertura { get; set; }
    public string? ObservacionesGenerales { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<AtencionDto> Atenciones { get; set; } = new();
}

public sealed class AtencionDto
{
    public int IdAtencion { get; set; }
    public int IdCita { get; set; }
    public int IdExpediente { get; set; }
    public int IdOdontologo { get; set; }
    public string NombreOdontologo { get; set; } = string.Empty;
    public DateTime FechaAtencion { get; set; }
    public string Diagnostico { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? Resultado { get; set; }
    public string? Recomendaciones { get; set; }
}

public sealed class DetalleAtencionDto
{
    public int IdAtencion { get; set; }
    public List<TratamientoAtencionDto> Tratamientos { get; set; } = new();
    public List<MedicamentoIndicadoDto> Medicamentos { get; set; } = new();
}

public sealed class TratamientoAtencionDto
{
    public int IdDetalleAtencion { get; set; }
    public int IdTratamiento { get; set; }
    public string NombreTratamiento { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioAplicado { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class MedicamentoIndicadoDto
{
    public int IdMedicamentoIndicado { get; set; }
    public int IdAtencion { get; set; }
    public string NombreMedicamento { get; set; } = string.Empty;
    public string? Dosis { get; set; }
    public string? Frecuencia { get; set; }
    public string? Duracion { get; set; }
    public string? Indicaciones { get; set; }
    public decimal Precio { get; set; }
}

public sealed class MedicamentoCatalogoDto
{
    public string NombreMedicamento { get; set; } = string.Empty;
    public string? Dosis { get; set; }
    public string? Frecuencia { get; set; }
    public string? Duracion { get; set; }
    public string? Indicaciones { get; set; }
    public decimal Precio { get; set; }
}

public sealed class ExamenDto
{
    public int IdExamen { get; set; }
    public int IdPaciente { get; set; }
    public int? IdAtencion { get; set; }
    public int IdOdontologo { get; set; }
    public string TipoExamen { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaRealizacion { get; set; }
    public string? Resultado { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public sealed class CreacionAtencionDto
{
    public string Diagnostico { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? Resultado { get; set; }
    public string? Recomendaciones { get; set; }
}

public sealed class ActualizacionAtencionDto
{
    public string Diagnostico { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? Resultado { get; set; }
    public string? Recomendaciones { get; set; }
}

public sealed class CreacionTratamientoAtencionDto
{
    public int IdTratamiento { get; set; }
    public string? Descripcion { get; set; }
    public decimal? PrecioAplicado { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class CreacionMedicamentoIndicadoDto
{
    public string NombreMedicamento { get; set; } = string.Empty;
    public string? Dosis { get; set; }
    public string? Frecuencia { get; set; }
    public string? Duracion { get; set; }
    public string? Indicaciones { get; set; }
    public decimal? Precio { get; set; }
}

public sealed class CreacionExamenDto
{
    public int? IdAtencion { get; set; }
    public string TipoExamen { get; set; } = string.Empty;
    public DateTime? FechaSolicitud { get; set; }
    public DateTime? FechaRealizacion { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class ActualizacionResultadoExamenDto
{
    public DateTime? FechaRealizacion { get; set; }
    public string? Resultado { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public sealed class ActualizacionExpedienteDto
{
    public string? ObservacionesGenerales { get; set; }
    public string Estado { get; set; } = string.Empty;
}
