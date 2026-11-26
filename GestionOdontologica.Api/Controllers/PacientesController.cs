using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PacientesController : ControllerBase
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IAntecedenteRepository _antecedenteRepository;

    public PacientesController(IPacienteRepository pacienteRepository, IAntecedenteRepository antecedenteRepository)
    {
        _pacienteRepository = pacienteRepository;
        _antecedenteRepository = antecedenteRepository;
    }

    [HttpGet]
    [Authorize(Policy = PermisoCodigos.PacientesVer)]
    public async Task<ActionResult<IReadOnlyList<PacienteDto>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        var pacientes = await _pacienteRepository.ObtenerTodosAsync(cancellationToken);
        return Ok(pacientes);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesVer)]
    public async Task<ActionResult<PacienteDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(id, cancellationToken);
        if (paciente is null)
        {
            return NotFound();
        }

        return Ok(paciente);
    }

    [HttpPost]
    [Authorize(Policy = PermisoCodigos.PacientesCrear)]
    public async Task<ActionResult<PacienteDto>> Crear([FromBody] CreacionPacienteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var paciente = await _pacienteRepository.InsertarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = paciente.IdPaciente }, paciente);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesEditar)]
    public async Task<ActionResult<PacienteDto>> Actualizar(int id, [FromBody] ActualizacionPacienteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var paciente = await _pacienteRepository.ActualizarAsync(id, dto, cancellationToken);
        if (paciente is null)
        {
            return NotFound();
        }

        return Ok(paciente);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisoCodigos.PacientesDesactivar)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _pacienteRepository.DesactivarAsync(id, cancellationToken);
        if (!resultado)
        {
            return NotFound();
        }

        return Ok(new { idPaciente = id, activo = false, mensaje = "Paciente desactivado correctamente." });
    }

    [HttpPost("{id:int}/activar")]
    [Authorize(Policy = PermisoCodigos.PacientesDesactivar)]
    public async Task<IActionResult> Activar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _pacienteRepository.ActivarAsync(id, cancellationToken);
        if (!resultado)
        {
            return NotFound();
        }

        return Ok(new { idPaciente = id, activo = true, mensaje = "Paciente activado correctamente." });
    }

    [HttpGet("{id:int}/antecedentes")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult<IReadOnlyList<AntecedenteDto>>> ObtenerAntecedentes(int id, CancellationToken cancellationToken)
    {
        var antecedentes = await _antecedenteRepository.ObtenerPorPacienteAsync(id, cancellationToken);
        return Ok(antecedentes);
    }

    [HttpPost("{id:int}/antecedentes")]
    [Authorize(Policy = PermisoCodigos.PacientesExpediente)]
    public async Task<ActionResult<AntecedenteDto>> CrearAntecedente(int id, [FromBody] CreacionAntecedenteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var antecedente = await _antecedenteRepository.InsertarAsync(id, dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerAntecedentes), new { id }, antecedente);
    }
}
