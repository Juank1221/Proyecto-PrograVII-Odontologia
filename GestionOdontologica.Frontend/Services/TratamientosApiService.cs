
using System.Net;
using System.Net.Http.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

public sealed class TratamientosApiService
{
    private readonly HttpClient _httpClient;

    public TratamientosApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<TratamientoDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<TratamientoDto>>(
            "tratamientos",
            cancellationToken);

        return resultado ?? new List<TratamientoDto>();
    }

    public async Task<TratamientoDto?> ObtenerPorIdAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"tratamientos/{idTratamiento}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TratamientoDto>(
            cancellationToken: cancellationToken);
    }

    public async Task<HttpResponseMessage> RegistrarAsync(
        CrearTratamientoRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsJsonAsync(
            "tratamientos",
            request,
            cancellationToken);
    }

    public async Task<HttpResponseMessage> ActualizarAsync(
        int idTratamiento,
        ActualizarTratamientoRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PutAsJsonAsync(
            $"tratamientos/{idTratamiento}",
            request,
            cancellationToken);
    }

    public async Task<HttpResponseMessage> DesactivarAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.DeleteAsync(
            $"tratamientos/{idTratamiento}",
            cancellationToken);
    }

    public async Task<HttpResponseMessage> ActivarAsync(
        int idTratamiento,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PutAsync(
            $"tratamientos/{idTratamiento}/activar",
            null,
            cancellationToken);
    }
}
