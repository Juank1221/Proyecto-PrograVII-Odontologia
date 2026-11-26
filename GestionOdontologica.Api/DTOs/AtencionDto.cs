namespace GestionOdontologica.Api.DTOs;

public sealed class AtencionDto
{
    public int IdAtencion { get; init; }
    public int IdCita { get; init; }
    public int IdExpediente { get; init; }
    public int IdOdontologo { get; init; }
    public string NombreOdontologo { get; init; } = string.Empty;
    public DateTime FechaAtencion { get; init; }
    public string Diagnostico { get; init; } = string.Empty;
    public string? Observaciones { get; init; }
    public string? Resultado { get; init; }
    public string? Recomendaciones { get; init; }
}
