
using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TratamientosController : ControllerBase
{
    private readonly ITratamientoRepository _tratamientoRepository;

    public TratamientosController(ITratamientoRepository tratamientoRepository)
    {
        _tratamientoRepository = tratamientoRepository;
    }

    [HttpGet]
    [Authorize(Policy = PermisoCodigos.TratamientosVer)]
    public async Task<ActionResult<IReadOnlyList<TratamientoDto>>> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var tratamientos = await _tratamientoRepository.ObtenerTodosAsync(
            cancellationToken);

        return Ok(tratamientos);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisoCodigos.TratamientosVer)]
    public async Task<ActionResult<TratamientoDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { mensaje = "El identificador del tratamiento no es válido." });

        var tratamiento = await _tratamientoRepository.ObtenerPorIdAsync(
            id,
            cancellationToken);

        if (tratamiento is null)
            return NotFound(new { mensaje = "No se encontró el tratamiento solicitado." });

        return Ok(tratamiento);
    }

    [HttpPost]
    [Authorize(Policy = PermisoCodigos.TratamientosCrear)]
    public async Task<IActionResult> Registrar(
        [FromBody] CrearTratamientoRequest request,
        CancellationToken cancellationToken)
    {
        var idTratamiento = await _tratamientoRepository.RegistrarAsync(
            request.CodigoTratamiento.Trim(),
            request.Nombre.Trim(),
            request.Descripcion?.Trim(),
            request.Precio,
            request.DuracionMinutos,
            cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = idTratamiento },
            new
            {
                idTratamiento,
                mensaje = "El tratamiento se registró correctamente."
            });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisoCodigos.TratamientosEditar)]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ActualizarTratamientoRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { mensaje = "El identificador del tratamiento no es válido." });

        var actualizado = await _tratamientoRepository.ActualizarAsync(
            id,
            request.CodigoTratamiento.Trim(),
            request.Nombre.Trim(),
            request.Descripcion?.Trim(),
            request.Precio,
            request.DuracionMinutos,
            cancellationToken);

        if (!actualizado)
            return NotFound(new { mensaje = "No se encontró el tratamiento que desea actualizar." });

        return Ok(new
        {
            mensaje = "El tratamiento se actualizó correctamente."
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisoCodigos.TratamientosDesactivar)]
    public async Task<IActionResult> Desactivar(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { mensaje = "El identificador del tratamiento no es válido." });

        var desactivado = await _tratamientoRepository.DesactivarAsync(
            id,
            cancellationToken);

        if (!desactivado)
            return NotFound(new { mensaje = "No se encontró un tratamiento activo con ese identificador." });

        return Ok(new
        {
            mensaje = "El tratamiento se desactivó correctamente."
        });
    }

    [HttpPut("{id:int}/activar")]
    [Authorize(Policy = PermisoCodigos.TratamientosEditar)]
    public async Task<IActionResult> Activar(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { mensaje = "El identificador del tratamiento no es válido." });

        var activado = await _tratamientoRepository.ActivarAsync(
            id,
            cancellationToken);

        if (!activado)
            return NotFound(new
            {
                mensaje = "No se encontró un tratamiento inactivo con ese identificador."
            });

        return Ok(new
        {
            mensaje = "El tratamiento se reactivó correctamente."
        });
    }
}
