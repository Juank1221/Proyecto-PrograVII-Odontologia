namespace GestionOdontologica.Api.DTOs;

public sealed class HorarioOdontologoDto
{
    public int IdHorario { get; init; }
    public int IdOdontologo { get; init; }
    public int DiaSemana { get; init; }
    public string HoraInicio { get; init; } = string.Empty;
    public string HoraFin { get; init; } = string.Empty;
    public bool Activo { get; init; }
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
