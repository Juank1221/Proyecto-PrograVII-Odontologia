using System.Globalization;
using System.Net.Http.Json;
using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

public sealed class BitacorasApiService
{
    private readonly HttpClient _httpClient;

    public BitacorasApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BitacoraPaginaDto> ConsultarAsync(BitacoraConsultaDto consulta, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.GetAsync($"bitacoras{Query(consulta)}", cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        return await respuesta.Content.ReadFromJsonAsync<BitacoraPaginaDto>(cancellationToken: cancellationToken)
            ?? new BitacoraPaginaDto();
    }

    public async Task<BitacoraResumenDto> ObtenerResumenAsync(CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.GetAsync("bitacoras/resumen", cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        return await respuesta.Content.ReadFromJsonAsync<BitacoraResumenDto>(cancellationToken: cancellationToken)
            ?? new BitacoraResumenDto();
    }

    public async Task<BitacoraAccesoResumenDto> ObtenerResumenAccesosAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var parametros = new List<string>();
        if (desde.HasValue) parametros.Add($"desde={desde.Value:yyyy-MM-dd}");
        if (hasta.HasValue) parametros.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        var query = parametros.Count > 0 ? "?" + string.Join('&', parametros) : string.Empty;

        var respuesta = await _httpClient.GetAsync($"bitacoras/accesos/resumen{query}", cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        return await respuesta.Content.ReadFromJsonAsync<BitacoraAccesoResumenDto>(cancellationToken: cancellationToken)
            ?? new BitacoraAccesoResumenDto();
    }

    public async Task<BitacoraFiltrosDto> ObtenerFiltrosAsync(CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.GetAsync("bitacoras/filtros", cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);
        return await respuesta.Content.ReadFromJsonAsync<BitacoraFiltrosDto>(cancellationToken: cancellationToken)
            ?? new BitacoraFiltrosDto();
    }

    public async Task<ExportacionBitacora> ExportarAsync(BitacoraConsultaDto consulta, CancellationToken cancellationToken = default)
    {
        var respuesta = await _httpClient.GetAsync($"bitacoras/exportar{Query(consulta)}", cancellationToken);
        await AsegurarExitoAsync(respuesta, cancellationToken);

        var nombre = respuesta.Content.Headers.ContentDisposition?.FileNameStar
            ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "bitacoras.csv";

        var bytes = await respuesta.Content.ReadAsByteArrayAsync(cancellationToken);
        return new ExportacionBitacora(nombre, bytes);
    }

    private static string Query(BitacoraConsultaDto consulta)
    {
        var partes = new List<string>();
        if (consulta.Desde.HasValue) partes.Add($"desde={consulta.Desde.Value:yyyy-MM-dd}");
        if (consulta.Hasta.HasValue) partes.Add($"hasta={consulta.Hasta.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(consulta.Modulo)) partes.Add($"modulo={Uri.EscapeDataString(consulta.Modulo!)}");
        if (!string.IsNullOrWhiteSpace(consulta.Resultado)) partes.Add($"resultado={Uri.EscapeDataString(consulta.Resultado!)}");
        if (!string.IsNullOrWhiteSpace(consulta.Texto)) partes.Add($"texto={Uri.EscapeDataString(consulta.Texto!)}");
        partes.Add($"pagina={Math.Max(consulta.Pagina, 1)}");
        partes.Add($"tamano={consulta.Tamano.ToString(CultureInfo.InvariantCulture)}");
        return "?" + string.Join('&', partes);
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
            // Respuesta sin ProblemDetails.
        }

        var status = (int)respuesta.StatusCode;
        var mensaje = problem?.Detail
            ?? problem?.Title
            ?? (status switch
            {
                403 => "No tienes permiso para consultar la bitácora.",
                404 => "No se encontró el registro solicitado.",
                >= 500 => "Error interno al consultar la bitácora.",
                _ => $"Error en la solicitud: {status} {respuesta.ReasonPhrase}"
            });

        throw new ApiException(mensaje, status, problem);
    }
}

public sealed record ExportacionBitacora(string Nombre, byte[] Contenido);
