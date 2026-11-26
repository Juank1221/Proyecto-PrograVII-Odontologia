using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Cliente HTTP del módulo de permisos (HU-20): catálogo y asignación a roles.
/// </summary>
public sealed class PermisosApiService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public PermisosApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<PermisoDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await EnviarAsync(
            () => _httpClient.GetAsync("permisos", cancellationToken),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return Array.Empty<PermisoDto>();

        await VerificarRespuestaAsync(response, cancellationToken);

        return await LeerAsync<List<PermisoDto>>(response, cancellationToken)
            ?? new List<PermisoDto>();
    }

    public async Task<PermisoDto> CrearAsync(
        CreacionPermisoDto dto,
        CancellationToken cancellationToken = default)
    {
        using var content = CrearContenido(dto);

        using var response = await EnviarAsync(
            () => _httpClient.PostAsync("permisos", content, cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);

        return await LeerAsync<PermisoDto>(response, cancellationToken)
            ?? throw new ApiException(
                "La API confirmó el alta pero no devolvió el permiso creado.",
                (int)response.StatusCode);
    }

    public async Task ActualizarAsync(
        int id,
        ActualizacionPermisoDto dto,
        CancellationToken cancellationToken = default)
    {
        using var content = CrearContenido(dto);

        using var response = await EnviarAsync(
            () => _httpClient.PutAsync($"permisos/{id}", content, cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);
    }

    public async Task EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await EnviarAsync(
            () => _httpClient.DeleteAsync($"permisos/{id}", cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<RolDto>> ListarRolesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await EnviarAsync(
            () => _httpClient.GetAsync("permisos/roles", cancellationToken),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return Array.Empty<RolDto>();

        await VerificarRespuestaAsync(response, cancellationToken);

        return await LeerAsync<List<RolDto>>(response, cancellationToken)
            ?? new List<RolDto>();
    }

    public async Task<PermisosRolDto> ObtenerPermisosRolAsync(
        int idRol,
        CancellationToken cancellationToken = default)
    {
        using var response = await EnviarAsync(
            () => _httpClient.GetAsync($"permisos/roles/{idRol}", cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);

        return await LeerAsync<PermisosRolDto>(response, cancellationToken)
            ?? throw new ApiException(
                "La API no devolvió los permisos del rol.",
                (int)response.StatusCode);
    }

    public async Task ActualizarPermisosRolAsync(
        int idRol,
        ActualizacionPermisosRolDto dto,
        CancellationToken cancellationToken = default)
    {
        using var content = CrearContenido(dto);

        using var response = await EnviarAsync(
            () => _httpClient.PutAsync(
                $"permisos/roles/{idRol}",
                content,
                cancellationToken),
            cancellationToken);

        await VerificarRespuestaAsync(response, cancellationToken);
    }

    private static StringContent CrearContenido(object dto)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);

        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<T?> LeerAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(
                JsonOptions,
                cancellationToken);
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
            // Un error sin JSON válido no debe ocultar el status HTTP.
        }

        var status = (int)response.StatusCode;

        var mensaje = status switch
        {
            400 => problem?.Detail ?? "Revisa los campos indicados.",
            401 => problem?.Detail ?? "Tu sesión expiró. Inicia sesión nuevamente.",
            403 => problem?.Detail ?? "Tu usuario no tiene permisos para realizar esta operación.",
            404 => problem?.Detail ?? "No se encontró el permiso o el rol solicitado.",
            409 => problem?.Detail ?? "La operación entra en conflicto con datos existentes.",
            >= 500 => problem?.Detail ?? "La API presentó un error interno.",
            _ => problem?.Detail ?? problem?.Title ?? $"Error HTTP {status}."
        };

        return new ApiException(mensaje, status, problem);
    }
}
