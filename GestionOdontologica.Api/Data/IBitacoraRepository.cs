using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IBitacoraRepository
{
    Task<BitacoraPaginaDto> ConsultarAsync(BitacoraConsultaDto consulta, CancellationToken cancellationToken = default);
    Task<BitacoraResumenDto> ObtenerResumenAsync(CancellationToken cancellationToken = default);
    Task<BitacoraAccesoResumenDto> ObtenerResumenAccesosAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);
    Task<BitacoraFiltrosDto> ObtenerFiltrosAsync(CancellationToken cancellationToken = default);
    Task RegistrarExportacionAsync(int idUsuario, int registros, CancellationToken cancellationToken = default);
    Task RegistrarAccesoAsync(
        int? idUsuario,
        string accion,
        string resultado,
        string? direccionIp,
        string detalle,
        CancellationToken cancellationToken = default);
}
