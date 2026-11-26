using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Cita devuelta por la agenda. Incluye los datos de paciente y odontólogo
/// necesarios para pintar la tarjeta sin hacer llamadas adicionales.
/// </summary>
public sealed class CitaDto
{
    public int IdCita { get; init; }
    public int IdPaciente { get; init; }
    public int IdOdontologo { get; init; }
    public int? IdTratamiento { get; init; }
    public DateTime FechaHoraInicio { get; init; }
    public DateTime FechaHoraFin { get; init; }
    public DateTime? FechaHoraLlegada { get; init; }
    public string? Motivo { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string? Observaciones { get; init; }
    public int DuracionMinutos { get; init; }
    public string NombrePaciente { get; init; } = string.Empty;
    public string? IdentificacionPaciente { get; init; }
    public string? TelefonoPaciente { get; init; }
    public string? CorreoPaciente { get; init; }
    public string NombreOdontologo { get; init; } = string.Empty;
    public string? NombreTratamiento { get; init; }

    /// <summary>Etiqueta amigable solicitada en HU-14.</summary>
    public string EstadoMostrado => Estado == "En espera"
        ? "En sala de espera"
        : Estado;

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
    [Required(ErrorMessage = "Selecciona un paciente.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un paciente válido.")]
    public int IdPaciente { get; set; }

    [Required(ErrorMessage = "Selecciona un odontólogo.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un odontólogo válido.")]
    public int IdOdontologo { get; set; }

    [Required(ErrorMessage = "Selecciona un tratamiento.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un tratamiento válido.")]
    public int IdTratamiento { get; set; }

    [Required(ErrorMessage = "Selecciona la fecha y hora de la cita.")]
    public DateTime FechaHoraInicio { get; set; }

    [StringLength(255, ErrorMessage = "El motivo no puede superar los 255 caracteres.")]
    public string? Motivo { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    public string? Observaciones { get; set; }
}

/// <summary>Bloque de tiempo del odontólogo para un tratamiento y una fecha.</summary>
public sealed class DisponibilidadCitaDto
{
    public string HoraInicio { get; init; } = string.Empty;
    public string HoraFin { get; init; } = string.Empty;

    /// <summary>DISPONIBLE u OCUPADO, tal como lo devuelve SP_OBTENER_SLOTS_DISPONIBLES.</summary>
    public string Estado { get; init; } = string.Empty;
    public bool Disponible { get; init; }
    public string Motivo { get; init; } = string.Empty;
}

/// <summary>
/// Respuesta de disponibilidad. Si el día no tiene bloques libres, incluye hasta
/// tres fechas posteriores con al menos un bloque libre.
/// </summary>
public sealed class DisponibilidadRespuestaDto
{
    public DateTime Fecha { get; init; }
    public bool HayDisponibilidad { get; init; }
    public IReadOnlyList<DisponibilidadCitaDto> Bloques { get; init; } = Array.Empty<DisponibilidadCitaDto>();
    public IReadOnlyList<DateTime> ProximasFechasDisponibles { get; init; } = Array.Empty<DateTime>();
}

/// <summary>Resultado de la validación de datos previa a confirmar (HU-13).</summary>
public sealed class ValidacionPacienteCitaDto
{
    public int IdPaciente { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string Identificacion { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public IReadOnlyList<string> CamposFaltantes { get; init; } = Array.Empty<string>();

    public bool EsValido => Activo && CamposFaltantes.Count == 0;
}

public sealed class NotificacionSalaEsperaDto
{
    public int IdNotificacion { get; init; }
    public int IdCita { get; init; }
    public int IdOdontologo { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public bool Leida { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime FechaHoraInicio { get; init; }
    public string NombrePaciente { get; init; } = string.Empty;
}

/// <summary>Cita pendiente de atencion clinica para el odontologo en sesion.</summary>
public sealed class CitaParaAtencionDto
{
    public int IdCita { get; init; }
    public int IdPaciente { get; init; }
    public int IdOdontologo { get; init; }
    public DateTime FechaHoraInicio { get; init; }
    public DateTime FechaHoraFin { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string? Motivo { get; init; }
    public int DuracionMinutos { get; init; }
    public string NombrePaciente { get; init; } = string.Empty;
    public string? NombreTratamiento { get; init; }
    public DateTime? FechaHoraInicioAtencion { get; init; }
}

/// <summary>Odontologo vinculado al usuario autenticado.</summary>
public sealed class MiOdontologoDto
{
    public int IdOdontologo { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
}
