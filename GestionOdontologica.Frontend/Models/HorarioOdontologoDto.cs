namespace GestionOdontologica.Frontend.Models;

public sealed class HorarioOdontologoDto
{
    public int IdHorario { get; set; }
    public int IdOdontologo { get; set; }
    public int DiaSemana { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class ConfiguracionHorariosDto
{
    public List<HorarioItemDto> Horarios { get; set; } = new();
}

public sealed class HorarioItemDto
{
    public int DiaSemana { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
