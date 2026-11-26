namespace GestionOdontologica.Api.DTOs;

public sealed class AntecedenteDto
{
    public int IdAntecedente { get; init; }
    public int IdPaciente { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string? Observaciones { get; init; }
    public DateTime FechaRegistro { get; init; }
}
