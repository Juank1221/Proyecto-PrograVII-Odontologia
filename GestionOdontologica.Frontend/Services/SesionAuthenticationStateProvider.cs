using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Puente entre <see cref="SesionService"/> y el sistema de autorización de
/// Blazor. La identidad se construye a partir de la sesión local y se vuelve a
/// notificar cuando el usuario inicia o cierra sesión.
/// </summary>
public sealed class SesionAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private const string TipoAutenticacion = "SesionLocal";

    private static readonly AuthenticationState Anonimo = new(
        new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly SesionService _sesion;

    public SesionAuthenticationStateProvider(SesionService sesion)
    {
        _sesion = sesion;
        _sesion.OnChange += AlCambiarSesion;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _sesion.InicializarAsync();

        if (_sesion.Usuario is not { } usuario || !usuario.EstaVigente)
        {
            return Anonimo;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("usuario", usuario.NombreUsuario),
            new Claim("expira_en", usuario.ExpiraEn.ToString("O"))
        };

        foreach (var permiso in usuario.Permisos)
        {
            claims.Add(new Claim("permiso", permiso));
        }

        var identidad = new ClaimsIdentity(claims, TipoAutenticacion, ClaimTypes.Name, ClaimTypes.Role);

        return new AuthenticationState(new ClaimsPrincipal(identidad));
    }

    public void Dispose()
    {
        _sesion.OnChange -= AlCambiarSesion;
    }

    private void AlCambiarSesion()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
