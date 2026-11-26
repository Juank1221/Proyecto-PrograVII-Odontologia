using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

/// <summary>
/// Administración de permisos (HU-20) y asignación de permisos a roles.
/// Todos los endpoints están protegidos por permisos reales verificados en el
/// backend; ocultar botones en el frontend es solo una ayuda visual.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class PermisosController : ControllerBase
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly IRolRepository _rolRepository;

    public PermisosController(
        IPermisoRepository permisoRepository,
        IRolRepository rolRepository)
    {
        _permisoRepository = permisoRepository;
        _rolRepository = rolRepository;
    }

    /// <summary>Lista todos los permisos con la cantidad de roles que los usan.</summary>
    [HttpGet]
    [Authorize(Policy = PermisoCodigos.PermisosVer)]
    [ProducesResponseType(typeof(IReadOnlyList<PermisoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PermisoDto>>> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var permisos = await _permisoRepository.ObtenerTodosAsync(cancellationToken);
        return Ok(permisos);
    }

    /// <summary>Obtiene un permiso por su identificador.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisoCodigos.PermisosVer)]
    [ProducesResponseType(typeof(PermisoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PermisoDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensaje = "El ID del permiso debe ser mayor que cero." });
        }

        var permiso = await _permisoRepository.ObtenerPorIdAsync(id, cancellationToken);

        if (permiso is null)
        {
            return NotFound(new { mensaje = "El permiso solicitado no existe." });
        }

        return Ok(permiso);
    }

    /// <summary>Crea un permiso.</summary>
    [HttpPost]
    [Authorize(Policy = PermisoCodigos.PermisosCrear)]
    [ProducesResponseType(typeof(PermisoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PermisoDto>> Crear(
        [FromBody] CreacionPermisoDto dto,
        CancellationToken cancellationToken)
    {
        var creado = await _permisoRepository.InsertarAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = creado.IdPermiso },
            creado);
    }

    /// <summary>Actualiza un permiso existente. El código interno se conserva.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisoCodigos.PermisosEditar)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ActualizacionPermisoDto dto,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensaje = "El ID del permiso debe ser mayor que cero." });
        }

        var actualizado = await _permisoRepository.ActualizarAsync(
            id,
            dto,
            cancellationToken);

        if (!actualizado)
        {
            return NotFound(new { mensaje = "El permiso no existe." });
        }

        return NoContent();
    }

    /// <summary>
    /// Elimina un permiso. La base de datos bloquea la operación si el permiso
    /// está asignado a algún rol (HTTP 409) para no perder relaciones.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisoCodigos.PermisosEliminar)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensaje = "El ID del permiso debe ser mayor que cero." });
        }

        var eliminado = await _permisoRepository.EliminarAsync(id, cancellationToken);

        if (!eliminado)
        {
            return NotFound(new { mensaje = "El permiso no existe." });
        }

        return NoContent();
    }

    /// <summary>Roles disponibles para la pantalla de asignación.</summary>
    [HttpGet("roles")]
    [Authorize(Policy = PermisoCodigos.PermisosVer)]
    [ProducesResponseType(typeof(IReadOnlyList<RolDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RolDto>>> ObtenerRoles(
        CancellationToken cancellationToken)
    {
        var roles = await _rolRepository.ObtenerTodosAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>Consulta el rol seleccionado y los ID de permisos que tiene.</summary>
    [HttpGet("roles/{idRol:int}")]
    [Authorize(Policy = PermisoCodigos.PermisosAsignar)]
    [ProducesResponseType(typeof(PermisosRolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PermisosRolDto>> ObtenerPermisosRol(
        int idRol,
        CancellationToken cancellationToken)
    {
        if (idRol <= 0)
        {
            return BadRequest(new { mensaje = "El ID del rol debe ser mayor que cero." });
        }

        var rol = await _rolRepository.ObtenerPorIdAsync(idRol, cancellationToken);

        if (rol is null)
        {
            return NotFound(new { mensaje = "El rol solicitado no existe." });
        }

        var ids = await _permisoRepository.ObtenerIdsPorRolAsync(
            idRol,
            cancellationToken);

        return Ok(new PermisosRolDto
        {
            IdRol = rol.IdRol,
            NombreRol = rol.NombreRol,
            Descripcion = rol.Descripcion,
            Activo = rol.Activo,
            IdPermisos = ids
        });
    }

    /// <summary>
    /// Reemplaza las asignaciones del rol en una sola transacción. Solo se
    /// insertan/eliminan las diferencias entre lo actual y lo solicitado.
    /// </summary>
    [HttpPut("roles/{idRol:int}")]
    [Authorize(Policy = PermisoCodigos.PermisosAsignar)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarPermisosRol(
        int idRol,
        [FromBody] ActualizacionPermisosRolDto dto,
        CancellationToken cancellationToken)
    {
        if (idRol <= 0)
        {
            return BadRequest(new { mensaje = "El ID del rol debe ser mayor que cero." });
        }

        var rol = await _rolRepository.ObtenerPorIdAsync(idRol, cancellationToken);

        if (rol is null)
        {
            return NotFound(new { mensaje = "El rol solicitado no existe." });
        }

        var ids = dto.IdPermisos
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        await _permisoRepository.ReemplazarAsignacionesAsync(
            idRol,
            ids,
            cancellationToken);

        return Ok(new
        {
            mensaje = $"Los permisos de {rol.NombreRol} se actualizaron correctamente.",
            idRol,
            idPermisos = ids
        });
    }

    /// <summary>Asigna un único permiso a un rol (operación idempotente).</summary>
    [HttpPost("roles/{idRol:int}/permisos/{idPermiso:int}")]
    [Authorize(Policy = PermisoCodigos.PermisosAsignar)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarPermiso(
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken)
    {
        if (idRol <= 0 || idPermiso <= 0)
        {
            return BadRequest(new { mensaje = "Los identificadores deben ser mayores que cero." });
        }

        var asignado = await _permisoRepository.AsignarAsync(
            idRol,
            idPermiso,
            cancellationToken);

        return Ok(new
        {
            mensaje = asignado
                ? "Permiso asignado correctamente."
                : "El rol ya tenía asignado ese permiso.",
            idRol,
            idPermiso,
            asignado
        });
    }

    /// <summary>Quita un permiso concreto a un rol.</summary>
    [HttpDelete("roles/{idRol:int}/permisos/{idPermiso:int}")]
    [Authorize(Policy = PermisoCodigos.PermisosAsignar)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QuitarPermiso(
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken)
    {
        if (idRol <= 0 || idPermiso <= 0)
        {
            return BadRequest(new { mensaje = "Los identificadores deben ser mayores que cero." });
        }

        var quitado = await _permisoRepository.QuitarAsync(
            idRol,
            idPermiso,
            cancellationToken);

        if (!quitado)
        {
            return NotFound(new { mensaje = "El rol no tenía asignado ese permiso." });
        }

        return NoContent();
    }
}
