
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

public sealed class RolesApiService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public RolesApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<RolDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await EnviarAsync(
            () => _httpClient.GetAsync("roles", cancellationToken),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return Array.Empty<RolDto>();

        await VerificarRespuestaAsync(response, cancellationToken);

        try
        {
            var roles = await response.Content.ReadFromJsonAsync<List<RolDto>>(
                JsonOptions,
                cancellationToken);

            return roles ?? new List<RolDto>();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new ApiException(
                "La API devolvió una respuesta con un formato inesperado.",
                (int)response.StatusCode,
                innerException: ex);
        }
    }

    public async Task<RolDto> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await EnviarAsync(
            () => _httpClient.GetAsync($"roles/{id}", cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);

        try
        {
            var rol = await response.Content.ReadFromJsonAsync<RolDto>(
                JsonOptions,
                cancellationToken);

            return rol ?? throw new ApiException(
                "La API no devolvió los datos del rol.",
                (int)response.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new ApiException(
                "La API devolvió una respuesta con un formato inesperado.",
                (int)response.StatusCode,
                innerException: ex);
        }
    }

    public async Task<RolDto> CrearAsync(
        CreacionRolDto dto,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await EnviarAsync(
            () => _httpClient.PostAsync("roles", content, cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);

        try
        {
            var creado = await response.Content.ReadFromJsonAsync<RolDto>(
                JsonOptions,
                cancellationToken);

            return creado ?? throw new ApiException(
                "La API confirmó el alta pero no devolvió los datos del rol.",
                (int)response.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new ApiException(
                "La API devolvió una respuesta con un formato inesperado.",
                (int)response.StatusCode,
                innerException: ex);
        }
    }

    public async Task ActualizarAsync(
        int id,
        ActualizacionRolDto dto,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await EnviarAsync(
            () => _httpClient.PutAsync($"roles/{id}", content, cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);
    }

    public async Task DesactivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await EnviarAsync(
            () => _httpClient.DeleteAsync($"roles/{id}", cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);
    }

    private static async Task<HttpResponseMessage> EnviarAsync(
        Func<Task<HttpResponseMessage>> solicitud,
        CancellationToken cancellationToken)
    {
        try
        {
            return await solicitud();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException(
                "La API tardó demasiado en responder. Verifica que el backend esté en ejecución.",
                innerException: ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(
                "No se pudo conectar con la API. Verifica que el backend esté en ejecución y que CORS permita este origen.",
                innerException: ex);
        }
    }

    private static async Task VerificarRespuestaAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await CrearErrorApiAsync(response, cancellationToken);
        }
    }

    private static async Task<ApiException> CrearErrorApiAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        ProblemDetailsDto? problem = null;

        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(
                JsonOptions,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
        }

        var status = (int)response.StatusCode;

        var mensaje = status switch
        {
            400 => problem?.Detail ?? "Revisa los campos indicados.",
            404 => problem?.Detail ?? "No se encontró el rol solicitado.",
            409 => problem?.Detail ?? "Ya existe un rol con ese nombre.",
            >= 500 => problem?.Detail ?? "La API presentó un error interno.",
            _ => problem?.Detail ?? problem?.Title ?? $"Error HTTP {status}."
        };

        return new ApiException(mensaje, status, problem);
    }
}
