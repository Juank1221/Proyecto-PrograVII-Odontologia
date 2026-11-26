using System.Security.Claims;
using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using GestionOdontologica.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

/// <summary>
/// Agenda clínica: creación con validación de reglas de negocio (HU-11),
/// disponibilidad por odontólogo/fecha (HU-12), validación de datos del
/// paciente (HU-13) y registro de llegada a sala de espera (HU-14).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class CitasController : ControllerBase
{
    private readonly ICitaRepository _citas;
    private readonly DisponibilidadCitasService _disponibilidad;

    public CitasController(ICitaRepository citas, DisponibilidadCitasService disponibilidad)
    {
        _citas = citas;
        _disponibilidad = disponibilidad;
    }

    [HttpGet]
    [Authorize(Policy = PermisoCodigos.CitasVer)]
    [ProducesResponseType(typeof(IReadOnlyList<CitaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> Listar(
        [FromQuery] DateTime? fecha,
        [FromQuery] int? idOdontologo,
        CancellationToken cancellationToken)
    {
        return Ok(await _citas.ListarAsync(fecha, idOdontologo, cancellationToken));
    }

    [HttpGet("disponibilidad")]
    [Authorize(Policy = PermisoCodigos.CitasVer)]
    [ProducesResponseType(typeof(DisponibilidadRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisponibilidadRespuestaDto>> ConsultarDisponibilidad(
        [FromQuery] DateTime fecha,
        [FromQuery] int odontologoId,
        [FromQuery] int tratamientoId,
        CancellationToken cancellationToken)
    {
        if (fecha == default)
        {
            return ValidationProblem("Debe indicar una fecha válida.");
        }

        if (odontologoId <= 0)
        {
            return ValidationProblem("Debe indicar un odontólogo válido.");
        }

        if (tratamientoId <= 0)
        {
            return ValidationProblem("Debe indicar un tratamiento válido.");
        }

        return Ok(await _disponibilidad.ObtenerAsync(
            odontologoId, fecha, tratamientoId, cancellationToken));
    }

    [HttpGet("validar-paciente/{idPaciente:int}")]
    [Authorize(Policy = PermisoCodigos.CitasVer)]
    [ProducesResponseType(typeof(ValidacionPacienteCitaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidacionPacienteCitaDto>> ValidarPaciente(
        int idPaciente,
        CancellationToken cancellationToken)
    {
        var validacion = await _citas.ValidarPacienteAsync(idPaciente, cancellationToken);

        if (validacion is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Paciente no encontrado",
                Detail = "No existe un paciente con el identificador indicado."
            });
        }

        return Ok(validacion);
    }

    [HttpGet("sala-espera")]
    [Authorize(Policy = PermisoCodigos.CitasVer)]
    [ProducesResponseType(typeof(IReadOnlyList<NotificacionSalaEsperaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificacionSalaEsperaDto>>> ListarSalaEspera(
        [FromQuery] int? idOdontologo,
        CancellationToken cancellationToken)
    {
        return Ok(await _citas.ListarSalaEsperaAsync(idOdontologo, cancellationToken));
    }

    [HttpPatch("notificaciones/{idNotificacion:int}/leida")]
    [Authorize(Policy = PermisoCodigos.CitasVer)]
    public async Task<IActionResult> MarcarNotificacionLeida(
        int idNotificacion,
        CancellationToken cancellationToken)
    {
        var actualizada = await _citas.MarcarNotificacionLeidaAsync(idNotificacion, cancellationToken);
        return actualizada ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisoCodigos.CitasVer)]
    [ProducesResponseType(typeof(CitaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CitaDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var cita = await _citas.ObtenerPorIdAsync(id, cancellationToken);
        return cita is null ? NotFound() : Ok(cita);
    }

    [HttpPost]
    [Authorize(Policy = PermisoCodigos.CitasCrear)]
    [ProducesResponseType(typeof(CitaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CitaDto>> Agendar(
        [FromBody] CrearCitaRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.FechaHoraInicio == default)
        {
            return ValidationProblem("Debe indicar la fecha y hora de la cita.");
        }

        // HU-13: la validación de datos se ejecuta también en el servidor para
        // que no se pueda omitir desde el cliente.
        var validacion = await _citas.ValidarPacienteAsync(request.IdPaciente, cancellationToken);

        if (validacion is null)
        {
            return DatosPacienteInvalidos(new Dictionary<string, string[]>
            {
                ["idPaciente"] = new[] { "Selecciona un paciente registrado." }
            });
        }

        if (!validacion.EsValido)
        {
            var errores = new Dictionary<string, string[]>();

            if (!validacion.Activo)
            {
                errores["paciente"] = new[] { "El paciente seleccionado está inactivo." };
            }

            foreach (var campo in validacion.CamposFaltantes)
            {
                errores[campo] = new[] { MensajeCampo(campo) };
            }

            return DatosPacienteInvalidos(errores);
        }

        var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idUsuario, out var idUsuarioRegistro))
        {
            return Unauthorized();
        }

        var idCita = await _citas.AgendarAsync(request, idUsuarioRegistro, cancellationToken);
        var cita = await _citas.ObtenerPorIdAsync(idCita, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, cita);
    }

    [HttpPatch("{id:int}/marcar-llegada")]
    [Authorize(Policy = PermisoCodigos.CitasEditar)]
    [ProducesResponseType(typeof(CitaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CitaDto>> MarcarLlegada(
        int id,
        CancellationToken cancellationToken)
    {
        var cita = await _citas.MarcarLlegadaAsync(id, cancellationToken);
        return cita is null ? NotFound() : Ok(cita);
    }

    [HttpGet("mi-odontologo")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    [ProducesResponseType(typeof(MiOdontologoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MiOdontologoDto>> ObtenerMiOdontologo(
        CancellationToken cancellationToken)
    {
        if (IdUsuarioSesion is not int idUsuario)
        {
            return Unauthorized();
        }

        var odontologo = await _citas.ObtenerMiOdontologoAsync(idUsuario, cancellationToken);
        return odontologo is null ? NotFound() : Ok(odontologo);
    }

    [HttpGet("para-atencion")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    [ProducesResponseType(typeof(IReadOnlyList<CitaParaAtencionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CitaParaAtencionDto>>> ListarParaAtencion(
        [FromQuery] int? idPaciente,
        [FromQuery] DateTime? fecha,
        CancellationToken cancellationToken)
    {
        if (IdUsuarioSesion is not int idUsuario)
        {
            return Unauthorized();
        }

        return Ok(await _citas.ListarParaAtencionAsync(
            idUsuario, idPaciente, fecha, cancellationToken));
    }

    [HttpPatch("{id:int}/iniciar-atencion")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    [ProducesResponseType(typeof(CitaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CitaDto>> IniciarAtencion(
        int id,
        CancellationToken cancellationToken)
    {
        if (IdUsuarioSesion is not int idUsuario)
        {
            return Unauthorized();
        }

        var cita = await _citas.IniciarAtencionAsync(idUsuario, id, cancellationToken);
        return cita is null ? NotFound() : Ok(cita);
    }

    [HttpPatch("{id:int}/finalizar-atencion")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    [ProducesResponseType(typeof(CitaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CitaDto>> FinalizarAtencion(
        int id,
        CancellationToken cancellationToken)
    {
        if (IdUsuarioSesion is not int idUsuario)
        {
            return Unauthorized();
        }

        var cita = await _citas.FinalizarAtencionAsync(idUsuario, id, cancellationToken);
        return cita is null ? NotFound() : Ok(cita);
    }

    private int? IdUsuarioSesion =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private ActionResult DatosPacienteInvalidos(Dictionary<string, string[]> errores)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Validación de datos del paciente",
            Detail = "Completa los datos marcados antes de confirmar la cita.",
            Instance = Request.Path
        };

        problem.Extensions["errors"] = errores;
        return StatusCode(StatusCodes.Status422UnprocessableEntity, problem);
    }

    private static string MensajeCampo(string campo) => campo switch
    {
        "nombreCompleto" => "El nombre completo del paciente es obligatorio.",
        "identificacion" => "La identificación del paciente es obligatoria.",
        "telefono" => "El teléfono del paciente es obligatorio.",
        "correo" => "El correo electrónico del paciente es obligatorio.",
        _ => "Completa este dato del paciente."
    };
}
