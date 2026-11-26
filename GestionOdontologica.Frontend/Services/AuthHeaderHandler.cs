using System.Net;
using System.Net.Http.Headers;
using GestionOdontologica.Frontend.Models;
using Microsoft.AspNetCore.Components;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Adjunta el token de la sesión a cada solicitud HTTP. Si el usuario no ha
/// iniciado sesión la solicitud sale sin cabecera y la API responde 401.
/// Cualquier 403 de la API (salvo en autenticación) muestra el aviso de acceso
/// denegado y redirige al inicio.
/// </summary>
public sealed class AuthHeaderHandler : DelegatingHandler
{
    private static readonly TimeSpan IntervaloAviso = TimeSpan.FromSeconds(3);

    private readonly SesionService _sesion;
    private readonly ToastService _toasts;
    private readonly NavigationManager _navegacion;
    private readonly ApiStatusService _estadoApi;
    private DateTime _ultimoAviso = DateTime.MinValue;

    public AuthHeaderHandler(
        SesionService sesion,
        ToastService toasts,
        NavigationManager navegacion,
        ApiStatusService estadoApi)
    {
        _sesion = sesion;
        _toasts = toasts;
        _navegacion = navegacion;
        _estadoApi = estadoApi;
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

        HttpResponseMessage respuesta;

        try
        {
            respuesta = await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            _estadoApi.Establecer(EstadoApi.SinConexion);
            throw;
        }

        // Cualquier respuesta HTTP demuestra que la API está en línea; solo un
        // 5xx indica que responde con error.
        _estadoApi.Establecer(respuesta.StatusCode >= HttpStatusCode.InternalServerError
            ? EstadoApi.Error
            : EstadoApi.EnLinea);

        if (respuesta.StatusCode == HttpStatusCode.Forbidden && !EsRutaDeAutenticacion(request))
        {
            NotificarAccesoDenegado();
        }

        return respuesta;
    }

    private void NotificarAccesoDenegado()
    {
        var ahora = DateTime.UtcNow;

        if (ahora - _ultimoAviso > IntervaloAviso)
        {
            _ultimoAviso = ahora;
            _toasts.Mostrar("Acceso denegado", RutasPermisos.MensajeAccesoDenegado, ToastTipo.Error);
        }

        var ruta = _navegacion.ToBaseRelativePath(_navegacion.Uri).Split('?', '#')[0].Trim('/');

        // Evita bucles: si ya estamos en inicio, solo se muestra el aviso.
        if (!string.Equals(ruta, "inicio", StringComparison.OrdinalIgnoreCase) && ruta.Length > 0)
        {
            _navegacion.NavigateTo("inicio", replace: true);
        }
    }

    private static bool EsRutaDeAutenticacion(HttpRequestMessage request) =>
        request.RequestUri?.AbsolutePath.Contains("/auth/", StringComparison.OrdinalIgnoreCase) == true;
}
