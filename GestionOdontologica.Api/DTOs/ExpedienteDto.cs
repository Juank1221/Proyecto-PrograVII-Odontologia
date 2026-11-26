namespace GestionOdontologica.Api.DTOs;

public sealed class ExpedienteDto
{
    public int IdExpediente { get; init; }
    public int IdPaciente { get; init; }
    public DateTime FechaApertura { get; init; }
    public string? ObservacionesGenerales { get; init; }
    public string Estado { get; init; } = string.Empty;
    public IReadOnlyList<AtencionDto> Atenciones { get; set; } = [];
}
