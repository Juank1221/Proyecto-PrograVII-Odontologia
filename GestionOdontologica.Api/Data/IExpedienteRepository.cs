using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IExpedienteRepository
{
    Task<ExpedienteDto?> ObtenerPorPacienteAsync(int idUsuario, int idPaciente, CancellationToken cancellationToken = default);
    Task ActualizarExpedienteAsync(int idUsuario, int idExpediente, ActualizacionExpedienteDto dto, CancellationToken cancellationToken = default);
    Task<int> RegistrarAtencionAsync(int idUsuario, int idCita, CreacionAtencionDto dto, CancellationToken cancellationToken = default);
    Task ActualizarAtencionAsync(int idUsuario, int idAtencion, ActualizacionAtencionDto dto, CancellationToken cancellationToken = default);
    Task<DetalleAtencionDto> ObtenerDetalleAtencionAsync(int idUsuario, int idAtencion, CancellationToken cancellationToken = default);
    Task<int> RegistrarTratamientoAsync(int idUsuario, int idAtencion, CreacionTratamientoAtencionDto dto, CancellationToken cancellationToken = default);
    Task ActualizarTratamientoAsync(int idUsuario, int idDetalleAtencion, CreacionTratamientoAtencionDto dto, CancellationToken cancellationToken = default);
    Task EliminarTratamientoAsync(int idUsuario, int idDetalleAtencion, CancellationToken cancellationToken = default);
    Task<int> RegistrarMedicamentoAsync(int idUsuario, int idAtencion, CreacionMedicamentoIndicadoDto dto, CancellationToken cancellationToken = default);
    Task ActualizarMedicamentoAsync(int idUsuario, int idMedicamento, CreacionMedicamentoIndicadoDto dto, CancellationToken cancellationToken = default);
    Task EliminarMedicamentoAsync(int idUsuario, int idMedicamento, CancellationToken cancellationToken = default);
    Task<int> RegistrarExamenAsync(int idUsuario, int idPaciente, CreacionExamenDto dto, CancellationToken cancellationToken = default);
    Task ActualizarResultadoExamenAsync(int idUsuario, int idExamen, ActualizacionResultadoExamenDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamenDto>> ObtenerExamenesPacienteAsync(int idUsuario, int idPaciente, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicamentoCatalogoDto>> ListarMedicamentosAsync(CancellationToken cancellationToken = default);
}
