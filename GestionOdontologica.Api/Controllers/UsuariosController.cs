using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
[Authorize]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuariosController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Obtiene todos los usuarios registrados (activos e inactivos).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PermisoCodigos.UsuariosVer)]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioRepository.ObtenerTodosAsync(cancellationToken);
        return Ok(usuarios);
    }

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisoCodigos.UsuariosVer)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id, cancellationToken);

        if (usuario is null)
        {
            return UsuarioNoEncontrado(id);
        }

        return Ok(usuario);
    }

    /// <summary>
    /// Crea un usuario. La contraseña se hashea con BCrypt antes de guardarla.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PermisoCodigos.UsuariosCrear)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsuarioDto>> Crear(
        [FromBody] CreacionUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        var creado = await _usuarioRepository.InsertarAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = creado.IdUsuario },
            creado);
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisoCodigos.UsuariosEditar)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsuarioDto>> Actualizar(
        int id,
        [FromBody] ActualizacionUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await _usuarioRepository.ActualizarAsync(
            id,
            dto,
            cancellationToken);

        if (!actualizado)
        {
            return UsuarioNoEncontrado(id);
        }

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id, cancellationToken);

        if (usuario is null)
        {
            // El usuario pudo eliminarse físicamente entre la actualización y la consulta.
            return UsuarioNoEncontrado(id);
        }

        return Ok(usuario);
    }

    /// <summary>
    /// Realiza la baja lógica de un usuario (activo = 0).
    /// El registro se conserva en la base de datos.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisoCodigos.UsuariosEliminar)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Desactivar(
        int id,
        CancellationToken cancellationToken)
    {
        var desactivado = await _usuarioRepository.DesactivarAsync(id, cancellationToken);

        if (!desactivado)
        {
            return UsuarioNoEncontrado(id);
        }

        return Ok(new
        {
            idUsuario = id,
            activo = false,
            mensaje = "Usuario desactivado correctamente."
        });
    }

    /// <summary>
    /// Reactiva un usuario previamente dado de baja (activo = 1).
    /// </summary>
    [HttpPost("{id:int}/reactivar")]
    [Authorize(Policy = PermisoCodigos.UsuariosReactivar)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reactivar(
        int id,
        CancellationToken cancellationToken)
    {
        var activado = await _usuarioRepository.ActivarAsync(id, cancellationToken);

        if (!activado)
        {
            return UsuarioNoEncontrado(id);
        }

        return Ok(new
        {
            idUsuario = id,
            activo = true,
            mensaje = "Usuario reactivado correctamente."
        });
    }

    [HttpGet("sin-odontologo")]
    [Authorize(Policy = PermisoCodigos.OdontologosVer)]
    public async Task<ActionResult<IReadOnlyList<UsuarioSinOdontologoDto>>> ObtenerSinOdontologo(CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioRepository.ObtenerSinOdontologoAsync(cancellationToken);
        return Ok(usuarios);
    }

    private ObjectResult UsuarioNoEncontrado(int id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Usuario no encontrado",
            detail: $"No existe un usuario con el identificador {id}.");
    }
}
