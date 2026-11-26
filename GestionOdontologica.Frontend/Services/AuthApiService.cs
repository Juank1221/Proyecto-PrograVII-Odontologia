using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Cliente HTTP de los endpoints públicos de autenticación y recuperación.
/// </summary>
public sealed class AuthApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<LoginResponse> IniciarSesionAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<LoginResponse>("auth/login", request, cancellationToken);
    }

    public Task<RecuperarPasswordResponse> SolicitarRecuperacionAsync(
        RecuperarPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<RecuperarPasswordResponse>(
            "auth/recuperar-password",
            request,
            cancellationToken);
    }

    public Task<MensajeResponse> RestablecerPasswordAsync(
        RestablecerPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<MensajeResponse>(
            "auth/restablecer-password",
            request,
            cancellationToken);
    }

    private async Task<T> PostAsync<T>(
        string ruta,
        object cuerpo,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(
                ruta,
                cuerpo,
                JsonOptions,
                cancellationToken);
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

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }

            try
            {
                var resultado = await response.Content.ReadFromJsonAsync<T>(
                    JsonOptions,
                    cancellationToken);

                return resultado ?? throw new ApiException(
                    "La API devolvió una respuesta vacía o con un formato inesperado.",
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

        var message = status switch
        {
            400 => problem?.Detail ?? "Revisa los datos ingresados.",
            401 => problem?.Detail ?? "El usuario o la contraseña no son correctos.",
            403 => problem?.Detail ?? "Tu cuenta no tiene acceso al sistema.",
            423 => problem?.Detail ?? "La cuenta está bloqueada temporalmente.",
            >= 500 => problem?.Detail ?? "La API presentó un error interno.",
            _ => problem?.Detail ?? problem?.Title ?? $"Error HTTP {status}."
        };

        return new ApiException(message, status, problem);
    }
}
