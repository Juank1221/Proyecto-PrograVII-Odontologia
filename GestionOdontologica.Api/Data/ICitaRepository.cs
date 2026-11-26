using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface ICitaRepository
{
    Task<IReadOnlyList<CitaDto>> ListarAsync(
        DateTime? fecha,
        int? idOdontologo,
        CancellationToken cancellationToken = default);

    Task<CitaDto?> ObtenerPorIdAsync(
        int idCita,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DisponibilidadCitaDto>> ConsultarDisponibilidadAsync(
        DateTime fecha,
        int idOdontologo,
        int idTratamiento,
        CancellationToken cancellationToken = default);

    Task<ValidacionPacienteCitaDto?> ValidarPacienteAsync(
        int idPaciente,
        CancellationToken cancellationToken = default);

    Task<int> AgendarAsync(
        CrearCitaRequest request,
        int idUsuarioRegistro,
        CancellationToken cancellationToken = default);

    Task<CitaDto?> MarcarLlegadaAsync(
        int idCita,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificacionSalaEsperaDto>> ListarSalaEsperaAsync(
        int? idOdontologo,
        CancellationToken cancellationToken = default);

    Task<bool> MarcarNotificacionLeidaAsync(
        int idNotificacion,
        CancellationToken cancellationToken = default);

    Task<MiOdontologoDto?> ObtenerMiOdontologoAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CitaParaAtencionDto>> ListarParaAtencionAsync(
        int idUsuario,
        int? idPaciente,
        DateTime? fecha,
        CancellationToken cancellationToken = default);

    Task<CitaDto?> IniciarAtencionAsync(
        int idUsuario,
        int idCita,
        CancellationToken cancellationToken = default);

    Task<CitaDto?> FinalizarAtencionAsync(
        int idUsuario,
        int idCita,
        CancellationToken cancellationToken = default);
}
