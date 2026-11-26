using System.Globalization;
using System.Security.Claims;
using System.Text;
using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

[ApiController]
[Route("api/bitacoras")]
[Authorize]
public sealed class BitacorasController : ControllerBase
{
    private const int MaximoExportacion = 5000;
    private readonly IBitacoraRepository _bitacoraRepository;

    public BitacorasController(IBitacoraRepository bitacoraRepository)
    {
        _bitacoraRepository = bitacoraRepository;
    }

    private int IdUsuarioSesion =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!, CultureInfo.InvariantCulture);

    [HttpGet]
    [Authorize(Policy = PermisoCodigos.BitacorasVer)]
    public async Task<ActionResult<BitacoraPaginaDto>> Consultar(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? modulo,
        [FromQuery] string? resultado,
        [FromQuery] string? texto,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 25,
        CancellationToken cancellationToken = default)
    {
        var paginaDto = await _bitacoraRepository.ConsultarAsync(new BitacoraConsultaDto
        {
            Desde = desde,
            Hasta = hasta?.Date.AddDays(1).AddSeconds(-1),
            Modulo = modulo,
            Resultado = resultado,
            Texto = texto,
            Pagina = pagina,
            Tamano = tamano
        }, cancellationToken);

        return Ok(paginaDto);
    }

    [HttpGet("resumen")]
    [Authorize(Policy = PermisoCodigos.BitacorasVer)]
    public async Task<ActionResult<BitacoraResumenDto>> Resumen(CancellationToken cancellationToken)
    {
        var resumen = await _bitacoraRepository.ObtenerResumenAsync(cancellationToken);
        return Ok(resumen);
    }

    [HttpGet("filtros")]
    [Authorize(Policy = PermisoCodigos.BitacorasVer)]
    public async Task<ActionResult<BitacoraFiltrosDto>> Filtros(CancellationToken cancellationToken)
    {
        var filtros = await _bitacoraRepository.ObtenerFiltrosAsync(cancellationToken);
        return Ok(filtros);
    }

    [HttpGet("accesos/resumen")]
    [Authorize(Policy = PermisoCodigos.BitacorasVer)]
    public async Task<ActionResult<BitacoraAccesoResumenDto>> ResumenAccesos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var resumen = await _bitacoraRepository.ObtenerResumenAccesosAsync(
            desde,
            hasta?.Date.AddDays(1).AddSeconds(-1),
            cancellationToken);

        return Ok(resumen);
    }

    [HttpGet("exportar")]
    [Authorize(Policy = PermisoCodigos.BitacorasExportar)]
    public async Task<IActionResult> Exportar(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? modulo,
        [FromQuery] string? resultado,
        [FromQuery] string? texto,
        CancellationToken cancellationToken)
    {
        var filas = new List<BitacoraDto>();
        var pagina = 1;

        while (filas.Count < MaximoExportacion)
        {
            var lote = await _bitacoraRepository.ConsultarAsync(new BitacoraConsultaDto
            {
                Desde = desde,
                Hasta = hasta?.Date.AddDays(1).AddSeconds(-1),
                Modulo = modulo,
                Resultado = resultado,
                Texto = texto,
                Pagina = pagina,
                Tamano = 100
            }, cancellationToken);

            filas.AddRange(lote.Items);
            if (lote.Items.Count == 0 || filas.Count >= lote.Total)
            {
                break;
            }

            pagina++;
        }

        filas = filas.Take(MaximoExportacion).ToList();
        await _bitacoraRepository.RegistrarExportacionAsync(IdUsuarioSesion, filas.Count, cancellationToken);

        var contenido = new StringBuilder();
        contenido.AppendLine("Fecha y hora;Modulo;Evento;Entidad;Usuario;Resultado;Direccion IP;Detalle");
        foreach (var fila in filas)
        {
            contenido.AppendLine(string.Join(';',
                Celda(fila.FechaHora.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                Celda(fila.Modulo),
                Celda(fila.Accion),
                Celda(fila.Entidad),
                Celda(fila.Usuario),
                Celda(fila.Resultado),
                Celda(string.IsNullOrWhiteSpace(fila.DireccionIp) ? "-" : fila.DireccionIp),
                Celda(fila.Detalle)));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(contenido.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8",
            $"bitacoras_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    /// <summary>Evita inyección de fórmulas al abrir el CSV en una hoja de cálculo.</summary>
    private static string Celda(string? valor)
    {
        var texto = (valor ?? string.Empty).Replace("\"", "\"\"");
        if (texto.Length > 0 && "=+-@".IndexOf(texto[0]) >= 0)
        {
            texto = "'" + texto;
        }

        return $"\"{texto}\"";
    }
}
