using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace GestionOdontologica.Api.Security;

/// <summary>
/// Esquema de autenticacion basado en el token de sesion emitido por el login.
/// El token viaja en la cabecera Authorization: Bearer.
/// </summary>
public sealed class SesionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Sesion";

    private readonly ISesionTokenService _tokenService;

    public SesionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISesionTokenService tokenService)
        : base(options, logger, encoder)
    {
        _tokenService = tokenService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var valores))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var cabecera = valores.ToString();

        if (!cabecera.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = cabecera["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Token de sesión vacío."));
        }

        var payload = _tokenService.ValidarToken(token);

        if (payload is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Token de sesión inválido o expirado."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, payload.IdUsuario.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, payload.NombreCompleto),
            new Claim(ClaimTypes.Email, payload.Correo),
            new Claim(ClaimTypes.Role, payload.Rol),
            new Claim("usuario", payload.NombreUsuario),
            new Claim("expira_en", payload.ExpiraEnUtc.ToString("O", CultureInfo.InvariantCulture))
        };

        var identidad = new ClaimsIdentity(claims, SchemeName, ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identidad);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
