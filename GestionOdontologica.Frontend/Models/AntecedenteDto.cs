namespace GestionOdontologica.Frontend.Models;

public sealed class AntecedenteDto
{
    public int IdAntecedente { get; set; }
    public int IdPaciente { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public sealed class CreacionAntecedenteDto
{
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}
