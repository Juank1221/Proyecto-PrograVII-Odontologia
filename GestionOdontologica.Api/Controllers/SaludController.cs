using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

/// <summary>
/// Comprobación de conectividad. Es pública y no toca la base de datos: sirve
/// para que el frontend confirme que la API responde.
/// </summary>
[ApiController]
[Route("api/salud")]
[AllowAnonymous]
[Produces("application/json")]
public sealed class SaludController : ControllerBase
{
    [HttpGet]
    public IActionResult Obtener() => Ok(new { estado = "En línea", fecha = DateTime.UtcNow });
}
