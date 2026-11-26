using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
[Authorize]
public sealed class CategoriasController : ControllerBase
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriasController(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    [HttpGet]
    [Authorize(Policy = PermisoCodigos.InventarioVer)]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> ObtenerTodas(CancellationToken cancellationToken)
    {
        return Ok(await _categoriaRepository.ObtenerTodasAsync(cancellationToken));
    }
}