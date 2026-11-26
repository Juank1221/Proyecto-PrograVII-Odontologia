using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Cliente HTTP de la Web API. Solo conoce los endpoints REST de usuarios:
/// GET /api/usuarios y POST /api/usuarios.
/// </summary>
public sealed class UsuariosApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public UsuariosApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.GetAsync("usuarios", cancellationToken);
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
            throw CrearErrorDeConexion(ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return Array.Empty<UsuarioDto>();
            }

            try
            {
                var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>(
                    JsonOptions,
                    cancellationToken);

                return usuarios ?? new List<UsuarioDto>();
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
    }

    public async Task<UsuarioDto> CrearAsync(
        CreacionUsuarioDto dto,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsync("usuarios", content, cancellationToken);
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
            throw CrearErrorDeConexion(ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }

            try
            {
                var creado = await response.Content.ReadFromJsonAsync<UsuarioDto>(
                    JsonOptions,
                    cancellationToken);

                if (creado is null)
                {
                    throw new ApiException(
                        "La API confirmó el alta pero no devolvió los datos del usuario.",
                        (int)response.StatusCode);
                }

                return creado;
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
    }

    public async Task<UsuarioDto> ActualizarAsync(
        int id,
        ActualizacionUsuarioDto dto,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PutAsync($"usuarios/{id}", content, cancellationToken);
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
            throw CrearErrorDeConexion(ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }

            try
            {
                var actualizado = await response.Content.ReadFromJsonAsync<UsuarioDto>(
                    JsonOptions,
                    cancellationToken);

                if (actualizado is null)
                {
                    throw new ApiException(
                        "La API confirmó la actualización pero no devolvió los datos del usuario.",
                        (int)response.StatusCode);
                }

                return actualizado;
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
    }

    public async Task DesactivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.DeleteAsync($"usuarios/{id}", cancellationToken);
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
            throw CrearErrorDeConexion(ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }
        }
    }

    public async Task ReactivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsync($"usuarios/{id}/reactivar", null, cancellationToken);
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
            throw CrearErrorDeConexion(ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }
        }
    }

    public async Task<UsuarioDto> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.GetAsync($"usuarios/{id}", cancellationToken);
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
            throw CrearErrorDeConexion(ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }

            try
            {
                var usuario = await response.Content.ReadFromJsonAsync<UsuarioDto>(
                    JsonOptions,
                    cancellationToken);

                if (usuario is null)
                {
                    throw new ApiException(
                        "No se encontraron los datos del usuario.",
                        (int)response.StatusCode);
                }

                return usuario;
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
    }

    public async Task<IReadOnlyList<UsuarioSinOdontologoDto>> ObtenerSinOdontologoAsync(
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.GetAsync("usuarios/sin-odontologo", cancellationToken);
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
            throw CrearErrorDeConexion(ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }

            try
            {
                var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioSinOdontologoDto>>(
                    JsonOptions,
                    cancellationToken);

                return usuarios ?? new List<UsuarioSinOdontologoDto>();
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
    }

    private static ApiException CrearErrorDeConexion(Exception ex)
    {
        return new ApiException(
            "No se pudo conectar con la API. Verifica que el backend esté en ejecución y que CORS permita este origen.",
            innerException: ex);
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
            // Una respuesta de error sin JSON válido no debe ocultar el status HTTP.
        }

        var status = (int)response.StatusCode;

        var message = status switch
        {
            400 => problem?.Detail ?? "Revisa los campos marcados.",
            404 => problem?.Detail ?? "No se encontró el recurso solicitado.",
            409 => problem?.Detail ?? "Ya existe un usuario con esos datos.",
            >= 500 => problem?.Detail ?? "La API presentó un error interno.",
            _ => problem?.Detail ?? problem?.Title ?? $"Error HTTP {status}."
        };

        return new ApiException(message, status, problem);
    }
}
