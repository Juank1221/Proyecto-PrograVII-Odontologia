using System.Net.Http.Json;
using System.Text.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

public sealed class CategoriasApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public CategoriasApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categorias = await _httpClient.GetFromJsonAsync<List<CategoriaDto>>("categorias", JsonOptions, cancellationToken);
            return categorias is null ? Array.Empty<CategoriaDto>() : categorias;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new ApiException("No fue posible cargar las categorías desde la API.", innerException: ex);
        }
    }
}
