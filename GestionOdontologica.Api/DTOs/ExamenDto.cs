namespace GestionOdontologica.Api.DTOs;

public sealed class ExamenDto
{
    public int IdExamen { get; init; }
    public int IdPaciente { get; init; }
    public int? IdAtencion { get; init; }
    public int IdOdontologo { get; init; }
    public string TipoExamen { get; init; } = string.Empty;
    public DateTime FechaSolicitud { get; init; }
    public DateTime? FechaRealizacion { get; init; }
    public string? Resultado { get; init; }
    public string? Observaciones { get; init; }
    public string Estado { get; init; } = string.Empty;
}
