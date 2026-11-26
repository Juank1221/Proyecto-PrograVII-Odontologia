using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IPacienteRepository
{
    Task<IReadOnlyList<PacienteDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<PacienteDto?> ObtenerPorIdAsync(int idPaciente, CancellationToken cancellationToken = default);
    Task<PacienteDto> InsertarAsync(CreacionPacienteDto dto, CancellationToken cancellationToken = default);
    Task<PacienteDto?> ActualizarAsync(int idPaciente, ActualizacionPacienteDto dto, CancellationToken cancellationToken = default);
    Task<bool> DesactivarAsync(int idPaciente, CancellationToken cancellationToken = default);
    Task<bool> ActivarAsync(int idPaciente, CancellationToken cancellationToken = default);
}
