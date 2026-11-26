
using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RolesController : ControllerBase
{
    private readonly IRolRepository _rolRepository;

    public RolesController(IRolRepository rolRepository)
    {
        _rolRepository = rolRepository;
    }

    [HttpGet]
    [Authorize(Policy = PermisoCodigos.RolesVer)]
    public async Task<ActionResult<IReadOnlyList<RolDto>>> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var roles = await _rolRepository.ObtenerTodosAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisoCodigos.RolesVer)]
    public async Task<ActionResult<RolDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { mensaje = "El ID del rol debe ser mayor que cero." });

        var rol = await _rolRepository.ObtenerPorIdAsync(id, cancellationToken);

        if (rol is null)
            return NotFound(new { mensaje = "El rol solicitado no existe." });

        return Ok(rol);
    }

    [HttpPost]
    [Authorize(Policy = PermisoCodigos.RolesCrear)]
    public async Task<ActionResult<RolDto>> Crear(
        [FromBody] CreacionRolDto dto,
        CancellationToken cancellationToken)
    {
        var rol = await _rolRepository.InsertarAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = rol.IdRol },
            rol);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisoCodigos.RolesEditar)]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ActualizacionRolDto dto,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { mensaje = "El ID del rol debe ser mayor que cero." });

        var actualizado = await _rolRepository.ActualizarAsync(
            id, dto, cancellationToken);

        if (!actualizado)
            return NotFound(new { mensaje = "El rol no existe." });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisoCodigos.RolesDesactivar)]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { mensaje = "El ID del rol debe ser mayor que cero." });

        var desactivado = await _rolRepository.DesactivarAsync(
            id, cancellationToken);

        if (!desactivado)
            return NotFound(new { mensaje = "El rol no existe o ya está inactivo." });

        return NoContent();
    }
}
