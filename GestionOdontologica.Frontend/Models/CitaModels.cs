namespace GestionOdontologica.Frontend.Models;

public sealed class CitaDto
{
    public int IdCita { get; set; }
    public int IdPaciente { get; set; }
    public int IdOdontologo { get; set; }
    public int? IdTratamiento { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public DateTime? FechaHoraLlegada { get; set; }
    public string? Motivo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public int DuracionMinutos { get; set; } = 30;
    public string NombrePaciente { get; set; } = string.Empty;
    public string? IdentificacionPaciente { get; set; }
    public string? TelefonoPaciente { get; set; }
    public string? CorreoPaciente { get; set; }
    public string NombreOdontologo { get; set; } = string.Empty;
    public string? NombreTratamiento { get; set; }
    public string EstadoMostrado => Estado == "En espera" ? "En sala de espera" : Estado;

    public string ClaseEstado => Estado switch
    {
        "Programada" => "programada",
        "Confirmada" => "confirmada",
        "En espera" => "espera",
        "En atencion" => "atencion",
        "Completada" => "completada",
        "Cancelada" => "cancelada",
        "Ausente" => "ausente",
        _ => "reprogramada"
    };
}

public sealed class CrearCitaRequest
{
    public int IdPaciente { get; set; }
    public int IdOdontologo { get; set; }
    public int IdTratamiento { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public string? Motivo { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class DisponibilidadCitaDto
{
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool Disponible { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public sealed class DisponibilidadRespuestaDto
{
    public DateTime Fecha { get; set; }
    public bool HayDisponibilidad { get; set; }
    public List<DisponibilidadCitaDto> Bloques { get; set; } = new();
    public List<DateTime> ProximasFechasDisponibles { get; set; } = new();
}

public sealed class ValidacionPacienteCitaDto
{
    public int IdPaciente { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public List<string> CamposFaltantes { get; set; } = new();
    public bool EsValido { get; set; }
}

public sealed class NotificacionSalaEsperaDto
{
    public int IdNotificacion { get; set; }
    public int IdCita { get; set; }
    public int IdOdontologo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public string NombrePaciente { get; set; } = string.Empty;
}

public sealed class CitaParaAtencionDto
{
    public int IdCita { get; set; }
    public int IdPaciente { get; set; }
    public int IdOdontologo { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public int DuracionMinutos { get; set; }
    public string NombrePaciente { get; set; } = string.Empty;
    public string? NombreTratamiento { get; set; }
    public DateTime? FechaHoraInicioAtencion { get; set; }
}

public sealed class MiOdontologoDto
{
    public int IdOdontologo { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
}
