using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OdontologosController : ControllerBase
{
    private readonly IOdontologoRepository _odontologoRepository;
    private readonly IHorarioOdontologoRepository _horarioRepository;
    private readonly IOdontologoTratamientoRepository _odontologoTratamientoRepository;

    public OdontologosController(
        IOdontologoRepository odontologoRepository,
        IHorarioOdontologoRepository horarioRepository,
        IOdontologoTratamientoRepository odontologoTratamientoRepository)
    {
        _odontologoRepository = odontologoRepository;
        _horarioRepository = horarioRepository;
        _odontologoTratamientoRepository = odontologoTratamientoRepository;
    }

    [HttpGet]
    [Authorize(Policy = PermisoCodigos.OdontologosVer)]
    public async Task<ActionResult<IReadOnlyList<OdontologoDto>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        var odontologos = await _odontologoRepository.ObtenerTodosAsync(cancellationToken);
        return Ok(odontologos);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisoCodigos.OdontologosVer)]
    public async Task<ActionResult<OdontologoDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var odontologo = await _odontologoRepository.ObtenerPorIdAsync(id, cancellationToken);
        if (odontologo is null)
        {
            return NotFound();
        }

        return Ok(odontologo);
    }

    [HttpPost]
    [Authorize(Policy = PermisoCodigos.OdontologosCrear)]
    public async Task<ActionResult<OdontologoDto>> Crear([FromBody] CreacionOdontologoDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var odontologo = await _odontologoRepository.InsertarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = odontologo.IdOdontologo }, odontologo);
    }

    [HttpPost("vinculado")]
    [Authorize(Policy = PermisoCodigos.OdontologosCrear)]
    public async Task<ActionResult<OdontologoDto>> CrearVinculado([FromBody] CreacionOdontologoVinculadoDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // Al crear un usuario nuevo desde la vinculación, todos los datos de
        // acceso son obligatorios; el procedimiento no debe recibir un usuario
        // sin credenciales.
        if (string.Equals(dto.TipoVinculacion?.Trim(), "CREAR_USUARIO", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(dto.NombreCompleto)
                || string.IsNullOrWhiteSpace(dto.NombreUsuario)
                || string.IsNullOrWhiteSpace(dto.CorreoUsuario)
                || string.IsNullOrWhiteSpace(dto.Password))
            {
                ModelState.AddModelError(
                    nameof(dto.Password),
                    "Para crear el usuario vinculado debes indicar nombre completo, nombre de usuario, correo y contraseña.");
                return ValidationProblem(ModelState);
            }
        }

        var odontologo = await _odontologoRepository.InsertarVinculadoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = odontologo.IdOdontologo }, odontologo);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisoCodigos.OdontologosEditar)]
    public async Task<ActionResult<OdontologoDto>> Actualizar(int id, [FromBody] ActualizacionOdontologoDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var odontologo = await _odontologoRepository.ActualizarAsync(id, dto, cancellationToken);
        if (odontologo is null)
        {
            return NotFound();
        }

        return Ok(odontologo);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisoCodigos.OdontologosDesactivar)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _odontologoRepository.DesactivarAsync(id, cancellationToken);
        if (!resultado)
        {
            return NotFound();
        }

        return Ok(new { idOdontologo = id, activo = false, mensaje = "Odontólogo desactivado correctamente." });
    }

    [HttpPost("{id:int}/activar")]
    [Authorize(Policy = PermisoCodigos.OdontologosDesactivar)]
    public async Task<IActionResult> Activar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _odontologoRepository.ActivarAsync(id, cancellationToken);
        if (!resultado)
        {
            return NotFound();
        }

        return Ok(new { idOdontologo = id, activo = true, mensaje = "Odontólogo activado correctamente." });
    }

    [HttpGet("{id:int}/horarios")]
    [Authorize(Policy = PermisoCodigos.OdontologosVer)]
    public async Task<ActionResult<IReadOnlyList<HorarioOdontologoDto>>> ObtenerHorarios(int id, CancellationToken cancellationToken)
    {
        var horarios = await _horarioRepository.ObtenerPorOdontologoAsync(id, cancellationToken);
        return Ok(horarios);
    }

    [HttpPut("{id:int}/horarios")]
    [Authorize(Policy = PermisoCodigos.OdontologosHorarios)]
    public async Task<IActionResult> ConfigurarHorarios(int id, [FromBody] ConfiguracionHorariosDto dto, CancellationToken cancellationToken)
    {
        await _horarioRepository.ReemplazarHorariosAsync(id, dto, cancellationToken);
        return Ok(new { mensaje = "Horarios configurados correctamente." });
    }

    [HttpGet("{id:int}/tratamientos")]
    [Authorize(Policy = PermisoCodigos.OdontologosVer)]
    public async Task<ActionResult<IReadOnlyList<TratamientoDto>>> ObtenerTratamientos(int id, CancellationToken cancellationToken)
    {
        var tratamientos = await _odontologoTratamientoRepository.ObtenerPorOdontologoAsync(id, cancellationToken);
        return Ok(tratamientos);
    }

    [HttpPost("{id:int}/tratamientos")]
    [Authorize(Policy = PermisoCodigos.OdontologosTratamientos)]
    public async Task<IActionResult> AsociarTratamiento(int id, [FromBody] AsociacionTratamientoDto dto, CancellationToken cancellationToken)
    {
        await _odontologoTratamientoRepository.AsociarAsync(id, dto.IdTratamiento, cancellationToken);
        return Ok(new { mensaje = "Tratamiento asociado correctamente." });
    }

    [HttpDelete("{id:int}/tratamientos/{idTratamiento:int}")]
    [Authorize(Policy = PermisoCodigos.OdontologosTratamientos)]
    public async Task<IActionResult> DesasociarTratamiento(int id, int idTratamiento, CancellationToken cancellationToken)
    {
        await _odontologoTratamientoRepository.DesasociarAsync(id, idTratamiento, cancellationToken);
        return Ok(new { mensaje = "Tratamiento desasociado correctamente." });
    }
}
