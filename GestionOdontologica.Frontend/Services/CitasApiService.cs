using System.Net;
using System.Net.Http.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

/// <summary>Cliente de la agenda: citas, disponibilidad, validación y llegada.</summary>
public sealed class CitasApiService
{
    private readonly HttpClient _httpClient;

    public CitasApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CitaDto>> ObtenerCitasAsync(
        DateTime? fecha = null,
        int? idOdontologo = null,
        CancellationToken cancellationToken = default)
    {
        var parametros = new List<string>();

        if (fecha.HasValue)
        {
            parametros.Add($"fecha={fecha.Value:yyyy-MM-dd}");
        }

        if (idOdontologo.HasValue)
        {
            parametros.Add($"idOdontologo={idOdontologo.Value}");
        }

        var url = parametros.Count == 0 ? "citas" : $"citas?{string.Join('&', parametros)}";
        var resultado = await _httpClient.GetFromJsonAsync<List<CitaDto>>(url, cancellationToken);
        return resultado ?? new List<CitaDto>();
    }

    public async Task<CitaDto?> ObtenerCitaAsync(
        int idCita,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<CitaDto>($"citas/{idCita}", cancellationToken);
    }

    public async Task<DisponibilidadRespuestaDto> ObtenerDisponibilidadAsync(
        int odontologoId,
        DateTime fecha,
        int tratamientoId,
        CancellationToken cancellationToken = default)
    {
        var url = $"citas/disponibilidad?fecha={fecha:yyyy-MM-dd}" +
                  $"&odontologoId={odontologoId}&tratamientoId={tratamientoId}";
        var resultado = await _httpClient.GetFromJsonAsync<DisponibilidadRespuestaDto>(url, cancellationToken);
        return resultado ?? new DisponibilidadRespuestaDto();
    }

    public async Task<ValidacionPacienteCitaDto?> ValidarPacienteAsync(
        int idPaciente,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<ValidacionPacienteCitaDto>(
            $"citas/validar-paciente/{idPaciente}", cancellationToken);
    }

    public async Task<CitaDto> CrearAsync(
        CrearCitaRequest request,
        CancellationToken cancellationToken = default)
    {
        using var respuesta = await _httpClient.PostAsJsonAsync("citas", request, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);

        return await respuesta.Content.ReadFromJsonAsync<CitaDto>(cancellationToken: cancellationToken)
               ?? throw new ApiException("No se pudo leer la respuesta al agendar la cita.");
    }

    public async Task<CitaDto> MarcarLlegadaAsync(
        int idCita,
        CancellationToken cancellationToken = default)
    {
        using var respuesta = await _httpClient.PatchAsync(
            $"citas/{idCita}/marcar-llegada",
            content: null,
            cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);

        return await respuesta.Content.ReadFromJsonAsync<CitaDto>(cancellationToken: cancellationToken)
               ?? throw new ApiException("No se pudo leer la respuesta al registrar la llegada.");
    }

    public async Task<IReadOnlyList<NotificacionSalaEsperaDto>> ListarSalaEsperaAsync(
        int? idOdontologo = null,
        CancellationToken cancellationToken = default)
    {
        var url = idOdontologo.HasValue
            ? $"citas/sala-espera?idOdontologo={idOdontologo.Value}"
            : "citas/sala-espera";

        var resultado = await _httpClient.GetFromJsonAsync<List<NotificacionSalaEsperaDto>>(url, cancellationToken);
        return resultado ?? new List<NotificacionSalaEsperaDto>();
    }

    public async Task MarcarNotificacionLeidaAsync(
        int idNotificacion,
        CancellationToken cancellationToken = default)
    {
        using var respuesta = await _httpClient.PatchAsync(
            $"citas/notificaciones/{idNotificacion}/leida",
            content: null,
            cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
    }

    public async Task<MiOdontologoDto?> ObtenerMiOdontologoAsync(
        CancellationToken cancellationToken = default)
    {
        using var respuesta = await _httpClient.GetAsync("citas/mi-odontologo", cancellationToken);
        if (!respuesta.IsSuccessStatusCode)
        {
            return null;
        }

        return await respuesta.Content.ReadFromJsonAsync<MiOdontologoDto>(
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<CitaParaAtencionDto>> ListarParaAtencionAsync(
        int? idPaciente = null,
        DateTime? fecha = null,
        CancellationToken cancellationToken = default)
    {
        var parametros = new List<string>();

        if (idPaciente.HasValue)
        {
            parametros.Add($"idPaciente={idPaciente.Value}");
        }

        if (fecha.HasValue)
        {
            parametros.Add($"fecha={fecha.Value:yyyy-MM-dd}");
        }

        var url = parametros.Count == 0
            ? "citas/para-atencion"
            : $"citas/para-atencion?{string.Join('&', parametros)}";

        using var respuesta = await _httpClient.GetAsync(url, cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);

        var resultado = await respuesta.Content.ReadFromJsonAsync<List<CitaParaAtencionDto>>(
            cancellationToken: cancellationToken);
        return resultado ?? new List<CitaParaAtencionDto>();
    }

    public async Task<CitaDto> IniciarAtencionAsync(
        int idCita,
        CancellationToken cancellationToken = default)
    {
        using var respuesta = await _httpClient.PatchAsync(
            $"citas/{idCita}/iniciar-atencion",
            content: null,
            cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);

        return await respuesta.Content.ReadFromJsonAsync<CitaDto>(cancellationToken: cancellationToken)
               ?? throw new ApiException("No se pudo leer la respuesta al iniciar la atención.");
    }

    public async Task<CitaDto> FinalizarAtencionAsync(
        int idCita,
        CancellationToken cancellationToken = default)
    {
        using var respuesta = await _httpClient.PatchAsync(
            $"citas/{idCita}/finalizar-atencion",
            content: null,
            cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);

        return await respuesta.Content.ReadFromJsonAsync<CitaDto>(cancellationToken: cancellationToken)
               ?? throw new ApiException("No se pudo leer la respuesta al finalizar la atención.");
    }

    private static async Task AsegurarExitoAsync(
        HttpResponseMessage respuesta,
        CancellationToken cancellationToken)
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
            // La API puede devolver texto plano si falla antes del middleware.
        }

        var status = (int)respuesta.StatusCode;
        var mensaje = problem?.Detail
            ?? problem?.Title
            ?? status switch
            {
                400 => "Revisa los datos de la cita.",
                401 or 403 => "Tu sesión no tiene permisos para esta acción.",
                404 => "No se encontró la cita solicitada.",
                409 => "El horario seleccionado ya está ocupado.",
                422 => "Completa los datos marcados del paciente.",
                >= 500 => "No fue posible completar la operación en la base de datos.",
                _ => $"Error en la solicitud: {status} {respuesta.ReasonPhrase}"
            };

        throw new ApiException(mensaje, status, problem);
    }
}
