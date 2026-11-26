using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

public sealed class ProductosApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public ProductosApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ProductoDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        // System.Text.Json no puede crear una instancia de IReadOnlyList<T>
        // directamente. La API devuelve un array JSON, por lo que se debe
        // deserializar primero a una colección concreta.
        return await EnviarAsync<List<ProductoDto>>(HttpMethod.Get, "productos", null, cancellationToken);
    }

    public Task<ProductoDto> CrearAsync(CreacionProductoDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync<ProductoDto>(HttpMethod.Post, "productos", dto, cancellationToken);

    public Task<SiguienteCodigoDto> ObtenerSiguienteCodigoAsync(CancellationToken cancellationToken = default) =>
        EnviarAsync<SiguienteCodigoDto>(HttpMethod.Get, "productos/siguiente-codigo", null, cancellationToken);

    public Task<ProductoDto> ActualizarAsync(int id, ActualizacionProductoDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync<ProductoDto>(HttpMethod.Put, $"productos/{id}", dto, cancellationToken);

    public async Task DesactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnviarAsync<object>(HttpMethod.Delete, $"productos/{id}", null, cancellationToken);
    }

    public async Task ReactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnviarAsync<object>(HttpMethod.Post, $"productos/{id}/reactivar", null, cancellationToken);
    }

    private async Task<T> EnviarAsync<T>(HttpMethod method, string uri, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException("La API tardó demasiado en responder. Verifica que el backend esté en ejecución.", innerException: ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("No fue posible conectar con la API. Verifica el backend y la configuración CORS.", innerException: ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CrearErrorApiAsync(response, cancellationToken);
            }

            if (typeof(T) == typeof(object) || response.StatusCode == HttpStatusCode.NoContent)
            {
                return (T)(object)new object();
            }

            try
            {
                var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
                return result ?? throw new ApiException("La API confirmó la operación pero no devolvió datos.", (int)response.StatusCode);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (JsonException ex)
            {
                throw new ApiException("La API devolvió una respuesta con un formato inesperado.", (int)response.StatusCode, innerException: ex);
            }
            catch (NotSupportedException ex)
            {
                throw new ApiException("La API devolvió una respuesta con un formato inesperado.", (int)response.StatusCode, innerException: ex);
            }
        }
    }

    private static async Task<ApiException> CrearErrorApiAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ProblemDetailsDto? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
        }

        var message = problem?.Detail
            ?? problem?.Title
            ?? response.ReasonPhrase
            ?? "La API devolvió un error.";

        return new ApiException(message, (int)response.StatusCode, problem);
    }
}
