namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Comprueba la conectividad con la API. El estado del topbar lo actualiza
/// <see cref="AuthHeaderHandler"/> con la respuesta de cada solicitud.
/// </summary>
public sealed class SaludApiService
{
    private readonly HttpClient _httpClient;

    public SaludApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ProbarConexionAsync(CancellationToken cancellationToken = default)
    {
        using var respuesta = await _httpClient.GetAsync("salud", cancellationToken);
    }
}
