using System.Globalization;
using System.Security.Claims;
using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/expedientes")]
[Authorize]
public sealed class ExpedientesController : ControllerBase
{
    private readonly IExpedienteRepository _expedienteRepository;

    public ExpedientesController(IExpedienteRepository expedienteRepository)
    {
        _expedienteRepository = expedienteRepository;
    }

    private int IdUsuarioSesion =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!, CultureInfo.InvariantCulture);

    [HttpGet("pacientes/{idPaciente:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult<ExpedienteDto>> ObtenerPorPaciente(int idPaciente, CancellationToken cancellationToken)
    {
        var expediente = await _expedienteRepository.ObtenerPorPacienteAsync(IdUsuarioSesion, idPaciente, cancellationToken);
        if (expediente is null)
        {
            return NotFound(new { mensaje = "El paciente no tiene expediente abierto." });
        }

        return Ok(expediente);
    }

    [HttpPut("{idExpediente:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<IActionResult> ActualizarExpediente(int idExpediente, [FromBody] ActualizacionExpedienteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _expedienteRepository.ActualizarExpedienteAsync(IdUsuarioSesion, idExpediente, dto, cancellationToken);
        return Ok(new { idExpediente, mensaje = "Expediente actualizado correctamente." });
    }

    [HttpPost("citas/{idCita:int}/atencion")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult> RegistrarAtencion(int idCita, [FromBody] CreacionAtencionDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var idAtencion = await _expedienteRepository.RegistrarAtencionAsync(IdUsuarioSesion, idCita, dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerDetalleAtencion), new { idAtencion },
            new { idAtencion, mensaje = "Atencion registrada correctamente." });
    }

    [HttpGet("atenciones/{idAtencion:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult<DetalleAtencionDto>> ObtenerDetalleAtencion(int idAtencion, CancellationToken cancellationToken)
    {
        var detalle = await _expedienteRepository.ObtenerDetalleAtencionAsync(IdUsuarioSesion, idAtencion, cancellationToken);
        return Ok(detalle);
    }

    [HttpPut("atenciones/{idAtencion:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<IActionResult> ActualizarAtencion(int idAtencion, [FromBody] ActualizacionAtencionDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _expedienteRepository.ActualizarAtencionAsync(IdUsuarioSesion, idAtencion, dto, cancellationToken);
        return Ok(new { idAtencion, mensaje = "Atencion actualizada correctamente." });
    }

    [HttpPost("atenciones/{idAtencion:int}/tratamientos")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult> RegistrarTratamiento(int idAtencion, [FromBody] CreacionTratamientoAtencionDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var idDetalle = await _expedienteRepository.RegistrarTratamientoAsync(IdUsuarioSesion, idAtencion, dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerDetalleAtencion), new { idAtencion },
            new { idDetalleAtencion = idDetalle, idAtencion, mensaje = "Tratamiento registrado correctamente." });
    }

    [HttpPost("atenciones/{idAtencion:int}/medicamentos")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult> RegistrarMedicamento(int idAtencion, [FromBody] CreacionMedicamentoIndicadoDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var idMedicamento = await _expedienteRepository.RegistrarMedicamentoAsync(IdUsuarioSesion, idAtencion, dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerDetalleAtencion), new { idAtencion },
            new { idMedicamentoIndicado = idMedicamento, idAtencion, mensaje = "Medicamento registrado correctamente." });
    }

    [HttpPut("tratamientos/{idDetalle:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<IActionResult> ActualizarTratamiento(
        int idDetalle,
        [FromBody] CreacionTratamientoAtencionDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _expedienteRepository.ActualizarTratamientoAsync(
            IdUsuarioSesion, idDetalle, dto, cancellationToken);

        return Ok(new { idDetalleAtencion = idDetalle, mensaje = "Tratamiento actualizado correctamente." });
    }

    [HttpDelete("tratamientos/{idDetalle:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<IActionResult> EliminarTratamiento(int idDetalle, CancellationToken cancellationToken)
    {
        await _expedienteRepository.EliminarTratamientoAsync(
            IdUsuarioSesion, idDetalle, cancellationToken);

        return Ok(new { idDetalleAtencion = idDetalle, mensaje = "Tratamiento eliminado correctamente." });
    }

    [HttpPut("medicamentos/{idMedicamento:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<IActionResult> ActualizarMedicamento(
        int idMedicamento,
        [FromBody] CreacionMedicamentoIndicadoDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _expedienteRepository.ActualizarMedicamentoAsync(
            IdUsuarioSesion, idMedicamento, dto, cancellationToken);

        return Ok(new { idMedicamentoIndicado = idMedicamento, mensaje = "Medicamento actualizado correctamente." });
    }

    [HttpDelete("medicamentos/{idMedicamento:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<IActionResult> EliminarMedicamento(int idMedicamento, CancellationToken cancellationToken)
    {
        await _expedienteRepository.EliminarMedicamentoAsync(
            IdUsuarioSesion, idMedicamento, cancellationToken);

        return Ok(new { idMedicamentoIndicado = idMedicamento, mensaje = "Medicamento eliminado correctamente." });
    }

    [HttpGet("medicamentos")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult<IReadOnlyList<MedicamentoCatalogoDto>>> ListarMedicamentos(
        CancellationToken cancellationToken)
    {
        return Ok(await _expedienteRepository.ListarMedicamentosAsync(cancellationToken));
    }

    [HttpGet("pacientes/{idPaciente:int}/examenes")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult<IReadOnlyList<ExamenDto>>> ObtenerExamenes(int idPaciente, CancellationToken cancellationToken)
    {
        var examenes = await _expedienteRepository.ObtenerExamenesPacienteAsync(IdUsuarioSesion, idPaciente, cancellationToken);
        return Ok(examenes);
    }

    [HttpPost("pacientes/{idPaciente:int}/examenes")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult> RegistrarExamen(int idPaciente, [FromBody] CreacionExamenDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var idExamen = await _expedienteRepository.RegistrarExamenAsync(IdUsuarioSesion, idPaciente, dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerExamenes), new { idPaciente },
            new { idExamen, idPaciente, mensaje = "Examen registrado correctamente." });
    }

    [HttpPut("examenes/{idExamen:int}/resultado")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<IActionResult> RegistrarResultadoExamen(int idExamen, [FromBody] ActualizacionResultadoExamenDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _expedienteRepository.ActualizarResultadoExamenAsync(IdUsuarioSesion, idExamen, dto, cancellationToken);
        return Ok(new { idExamen, mensaje = "Resultado registrado correctamente." });
    }
}
