using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Produces("application/json")]
[Authorize]
public sealed class ProductosController : ControllerBase
{
    private readonly IProductoRepository _productoRepository;

    public ProductosController(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    [HttpGet]
    [Authorize(Policy = PermisoCodigos.InventarioVer)]
    [ProducesResponseType(typeof(IReadOnlyList<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        return Ok(await _productoRepository.ObtenerTodosAsync(cancellationToken));
    }

    [HttpGet("siguiente-codigo")]
    [Authorize(Policy = PermisoCodigos.InventarioCrear)]
    [ProducesResponseType(typeof(SiguienteCodigoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SiguienteCodigoDto>> ObtenerSiguienteCodigo(
        CancellationToken cancellationToken)
    {
        var codigo = await _productoRepository.ObtenerSiguienteCodigoAsync(cancellationToken);
        return Ok(new SiguienteCodigoDto { Codigo = codigo });
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisoCodigos.InventarioVer)]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensaje = "El ID del producto debe ser mayor que cero." });
        }

        var producto = await _productoRepository.ObtenerPorIdAsync(id, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    [Authorize(Policy = PermisoCodigos.InventarioCrear)]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoDto>> Crear(
        [FromBody] CreacionProductoDto dto,
        CancellationToken cancellationToken)
    {
        var producto = await _productoRepository.InsertarAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = producto.IdProducto },
            producto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisoCodigos.InventarioEditar)]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoDto>> Actualizar(
        int id,
        [FromBody] ActualizacionProductoDto dto,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensaje = "El ID del producto debe ser mayor que cero." });
        }

        var actualizado = await _productoRepository.ActualizarAsync(id, dto, cancellationToken);

        if (!actualizado)
        {
            return NotFound();
        }

        var producto = await _productoRepository.ObtenerPorIdAsync(id, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisoCodigos.InventarioDesactivar)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensaje = "El ID del producto debe ser mayor que cero." });
        }

        await _productoRepository.DesactivarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reactivar")]
    [Authorize(Policy = PermisoCodigos.InventarioDesactivar)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reactivar(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensaje = "El ID del producto debe ser mayor que cero." });
        }

        await _productoRepository.ActivarAsync(id, cancellationToken);
        return NoContent();
    }
}
