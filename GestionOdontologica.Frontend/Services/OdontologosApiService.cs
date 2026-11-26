using System.Net.Http.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

public sealed class OdontologosApiService
{
    private readonly HttpClient _httpClient;

    public OdontologosApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<OdontologoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<OdontologoDto>>("odontologos", cancellationToken);
        return resultado ?? new List<OdontologoDto>();
    }

    public async Task<OdontologoDto> CrearAsync(CreacionOdontologoDto dto, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PostAsJsonAsync("odontologos", dto, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        var odontologo = await respuesta.Content.ReadFromJsonAsync<OdontologoDto>(cancellationToken: cancellationToken);
        return odontologo ?? throw new ApiException("No se pudo leer la respuesta al crear el odontólogo.");
    }

    public async Task<OdontologoDto> CrearVinculadoAsync(CreacionOdontologoVinculadoDto dto, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PostAsJsonAsync("odontologos/vinculado", dto, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        var odontologo = await respuesta.Content.ReadFromJsonAsync<OdontologoDto>(cancellationToken: cancellationToken);
        return odontologo ?? throw new ApiException("No se pudo leer la respuesta al crear el odontólogo vinculado.");
    }

    public async Task<OdontologoDto> ActualizarAsync(int id, ActualizacionOdontologoDto dto, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PutAsJsonAsync($"odontologos/{id}", dto, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        var odontologo = await respuesta.Content.ReadFromJsonAsync<OdontologoDto>(cancellationToken: cancellationToken);
        return odontologo ?? throw new ApiException("No se pudo leer la respuesta al actualizar el odontólogo.");
    }

    public async Task DesactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.DeleteAsync($"odontologos/{id}", cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    public async Task ActivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PostAsync($"odontologos/{id}/activar", null, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    public async Task<IReadOnlyList<HorarioOdontologoDto>> ObtenerHorariosAsync(int id, CancellationToken cancellationToken = default)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<HorarioOdontologoDto>>($"odontologos/{id}/horarios", cancellationToken);
        return resultado ?? new List<HorarioOdontologoDto>();
    }

    public async Task ConfigurarHorariosAsync(int id, ConfiguracionHorariosDto dto, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PutAsJsonAsync($"odontologos/{id}/horarios", dto, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    public async Task<IReadOnlyList<TratamientoDto>> ObtenerTratamientosAsociadosAsync(int id, CancellationToken cancellationToken = default)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<TratamientoDto>>($"odontologos/{id}/tratamientos", cancellationToken);
        return resultado ?? new List<TratamientoDto>();
    }

    public async Task AsociarTratamientoAsync(int id, int idTratamiento, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PostAsJsonAsync($"odontologos/{id}/tratamientos", new AsociacionTratamientoDto { IdTratamiento = idTratamiento }, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    public async Task DesasociarTratamientoAsync(int id, int idTratamiento, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.DeleteAsync($"odontologos/{id}/tratamientos/{idTratamiento}", cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    private static async Task AsegurarExitoAsync(HttpResponseMessage respuesta, CancellationToken cancellationToken)
    {
        if (respuesta.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetailsDto? problem = null;
        try
        {
            problem = await respuesta.Content.ReadFromJsonAsync<ProblemDetailsDto>(cancellationToken: cancellationToken);
        }
        catch
        {
            // Fallback a lectura de texto plano si no es ProblemDetails
        }

        var status = (int)respuesta.StatusCode;
        var mensaje = problem?.Detail
            ?? problem?.Title
            ?? (status switch
            {
                400 => "Revisa los datos ingresados.",
                404 => "No se encontró el odontólogo o recurso solicitado.",
                409 => "Ya existe un odontólogo con ese colegiado o correo.",
                >= 500 => "Error interno en el servidor de base de datos.",
                _ => $"Error en la solicitud: {status} {respuesta.ReasonPhrase}"
            });

        throw new ApiException(mensaje, status, problem);
    }
}
