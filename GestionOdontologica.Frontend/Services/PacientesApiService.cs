using System.Net.Http.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

public sealed class PacientesApiService
{
    private readonly HttpClient _httpClient;

    public PacientesApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<PacienteDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<PacienteDto>>("pacientes", cancellationToken);
        return resultado ?? new List<PacienteDto>();
    }

    public async Task<PacienteDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<PacienteDto>($"pacientes/{id}", cancellationToken);
    }

    public async Task<PacienteDto> CrearAsync(CreacionPacienteDto dto, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PostAsJsonAsync("pacientes", dto, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        var paciente = await respuesta.Content.ReadFromJsonAsync<PacienteDto>(cancellationToken: cancellationToken);
        return paciente ?? throw new ApiException("No se pudo leer la respuesta al crear el paciente.");
    }

    public async Task<PacienteDto> ActualizarAsync(int id, ActualizacionPacienteDto dto, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PutAsJsonAsync($"pacientes/{id}", dto, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        var paciente = await respuesta.Content.ReadFromJsonAsync<PacienteDto>(cancellationToken: cancellationToken);
        return paciente ?? throw new ApiException("No se pudo leer la respuesta al actualizar el paciente.");
    }

    public async Task DesactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.DeleteAsync($"pacientes/{id}", cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    public async Task ActivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PostAsync($"pacientes/{id}/activar", null, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    public async Task<IReadOnlyList<AntecedenteDto>> ObtenerAntecedentesAsync(int id, CancellationToken cancellationToken = default)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<AntecedenteDto>>($"pacientes/{id}/antecedentes", cancellationToken);
        return resultado ?? new List<AntecedenteDto>();
    }

    public async Task<AntecedenteDto> CrearAntecedenteAsync(int id, CreacionAntecedenteDto dto, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.PostAsJsonAsync($"pacientes/{id}/antecedentes", dto, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        var antecedente = await respuesta.Content.ReadFromJsonAsync<AntecedenteDto>(cancellationToken: cancellationToken);
        return antecedente ?? throw new ApiException("No se pudo leer la respuesta al crear el antecedente.");
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
                404 => "No se encontró el paciente solicitado.",
                409 => "Ya existe un paciente con esa identificación o correo.",
                >= 500 => "Error interno en el servidor de base de datos.",
                _ => $"Error en la solicitud: {status} {respuesta.ReasonPhrase}"
            });

        throw new ApiException(mensaje, status, problem);
    }
}
