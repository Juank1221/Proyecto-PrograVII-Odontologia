using System.Net.Http.Headers;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Adjunta el token de la sesión a cada solicitud HTTP. Si el usuario no ha
/// iniciado sesión la solicitud sale sin cabecera y la API responde 401.
/// </summary>
public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly SesionService _sesion;

    public AuthHeaderHandler(SesionService sesion)
    {
        _sesion = sesion;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await _sesion.InicializarAsync();

        var token = _sesion.Token;

        if (!string.IsNullOrWhiteSpace(token)
            && request.Headers.Authorization is null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
