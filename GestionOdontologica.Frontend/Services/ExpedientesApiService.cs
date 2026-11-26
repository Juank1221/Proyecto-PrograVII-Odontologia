using System.Net;
using System.Net.Http.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

public sealed class ExpedientesApiService
{
    private readonly HttpClient _httpClient;

    public ExpedientesApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>Devuelve null cuando el paciente aun no tiene expediente abierto.</summary>
    public async Task<ExpedienteDto?> ObtenerPorPacienteAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.GetAsync($"expedientes/pacientes/{idPaciente}", cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await AsegurarExitoAsync(respuesta, cancellationToken);
        return await respuesta.Content.ReadFromJsonAsync<ExpedienteDto>(cancellationToken: cancellationToken);
    }

    public Task ActualizarExpedienteAsync(int idExpediente, ActualizacionExpedienteDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Put, $"expedientes/{idExpediente}", dto, cancellationToken);

    public Task RegistrarAtencionAsync(int idCita, CreacionAtencionDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Post, $"expedientes/citas/{idCita}/atencion", dto, cancellationToken);

    public Task ActualizarAtencionAsync(int idAtencion, ActualizacionAtencionDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Put, $"expedientes/atenciones/{idAtencion}", dto, cancellationToken);

    public async Task<DetalleAtencionDto> ObtenerDetalleAtencionAsync(int idAtencion, CancellationToken cancellationToken = default)
    {
        var detalle = await _httpClient.GetFromJsonAsync<DetalleAtencionDto>($"expedientes/atenciones/{idAtencion}", cancellationToken);
        return detalle ?? new DetalleAtencionDto { IdAtencion = idAtencion };
    }

    public Task RegistrarTratamientoAsync(int idAtencion, CreacionTratamientoAtencionDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Post, $"expedientes/atenciones/{idAtencion}/tratamientos", dto, cancellationToken);

    public Task ActualizarTratamientoAsync(int idDetalleAtencion, CreacionTratamientoAtencionDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Put, $"expedientes/tratamientos/{idDetalleAtencion}", dto, cancellationToken);

    public Task EliminarTratamientoAsync(int idDetalleAtencion, CancellationToken cancellationToken = default) =>
        EnviarSinCuerpoAsync(HttpMethod.Delete, $"expedientes/tratamientos/{idDetalleAtencion}", cancellationToken);

    public Task RegistrarMedicamentoAsync(int idAtencion, CreacionMedicamentoIndicadoDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Post, $"expedientes/atenciones/{idAtencion}/medicamentos", dto, cancellationToken);

    public Task ActualizarMedicamentoAsync(int idMedicamento, CreacionMedicamentoIndicadoDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Put, $"expedientes/medicamentos/{idMedicamento}", dto, cancellationToken);

    public Task EliminarMedicamentoAsync(int idMedicamento, CancellationToken cancellationToken = default) =>
        EnviarSinCuerpoAsync(HttpMethod.Delete, $"expedientes/medicamentos/{idMedicamento}", cancellationToken);

    public async Task<IReadOnlyList<MedicamentoCatalogoDto>> ListarMedicamentosAsync(CancellationToken cancellationToken = default)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<MedicamentoCatalogoDto>>(
            "expedientes/medicamentos", cancellationToken);
        return resultado ?? new List<MedicamentoCatalogoDto>();
    }

    public async Task<IReadOnlyList<ExamenDto>> ObtenerExamenesAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<ExamenDto>>($"expedientes/pacientes/{idPaciente}/examenes", cancellationToken);
        return resultado ?? new List<ExamenDto>();
    }

    public Task RegistrarExamenAsync(int idPaciente, CreacionExamenDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Post, $"expedientes/pacientes/{idPaciente}/examenes", dto, cancellationToken);

    public Task RegistrarResultadoExamenAsync(int idExamen, ActualizacionResultadoExamenDto dto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Put, $"expedientes/examenes/{idExamen}/resultado", dto, cancellationToken);

    private async Task EnviarAsync<T>(HttpMethod metodo, string ruta, T dto, CancellationToken cancellationToken)
    {
        using var solicitud = new HttpRequestMessage(metodo, ruta)
        {
            Content = JsonContent.Create(dto)
        };

        var respuesta = await _httpClient.SendAsync(solicitud, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    private async Task EnviarSinCuerpoAsync(HttpMethod metodo, string ruta, CancellationToken cancellationToken)
    {
        using var solicitud = new HttpRequestMessage(metodo, ruta);
        using var respuesta = await _httpClient.SendAsync(solicitud, cancellationToken);
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
            // La respuesta no es ProblemDetails; se usa el mensaje por defecto.
        }

        var status = (int)respuesta.StatusCode;
        var mensaje = problem?.Detail
            ?? problem?.Title
            ?? (status switch
            {
                400 => "Revisa los datos ingresados.",
                403 => "No tienes permiso para realizar esta operación.",
                404 => "No se encontró el registro solicitado.",
                409 => "El registro ya existe o entra en conflicto con otro.",
                >= 500 => "Error interno en el servidor de base de datos.",
                _ => $"Error en la solicitud: {status} {respuesta.ReasonPhrase}"
            });

        throw new ApiException(mensaje, status, problem);
    }
}
