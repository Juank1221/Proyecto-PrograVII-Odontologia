using System.Security.Claims;
using GestionOdontologica.Api.Data;
using Microsoft.AspNetCore.Authorization;

namespace GestionOdontologica.Api.Security;

/// <summary>
/// Resuelve los permisos reales del usuario en la base de datos en cada
/// solicitud. Se deniega por defecto: si no se puede verificar el permiso, no
/// se autoriza la operacion.
/// </summary>
public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private const string ClaveCache = "odontocare.permisos";

    private readonly IPermisoRepository _permisoRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IPermisoRepository permisoRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _permisoRepository = permisoRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var idTexto = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idTexto, out var idUsuario) || idUsuario <= 0)
        {
            _logger.LogWarning("Token autenticado sin identificador de usuario válido.");
            return;
        }

        try
        {
            var codigos = await ObtenerCodigosAsync(idUsuario);

            if (codigos.Contains(requirement.Codigo))
            {
                context.Succeed(requirement);
            }
        }
        catch (Exception ex)
        {
            // Fail closed: un error de consulta nunca concede acceso.
            _logger.LogError(
                ex,
                "No fue posible verificar el permiso {Codigo} para el usuario {IdUsuario}.",
                requirement.Codigo,
                idUsuario);
        }
    }

    private async Task<HashSet<string>> ObtenerCodigosAsync(int idUsuario)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Items[ClaveCache] is HashSet<string> cache)
        {
            return cache;
        }

        var token = httpContext?.RequestAborted ?? CancellationToken.None;

        var codigos = await _permisoRepository.ObtenerCodigosPorUsuarioAsync(
            idUsuario,
            token);

        var conjunto = new HashSet<string>(codigos, StringComparer.OrdinalIgnoreCase);

        if (httpContext is not null)
        {
            httpContext.Items[ClaveCache] = conjunto;
        }

        return conjunto;
    }
}
