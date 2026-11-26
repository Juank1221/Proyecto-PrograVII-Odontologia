using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public interface IAntecedenteRepository
{
    Task<IReadOnlyList<AntecedenteDto>> ObtenerPorPacienteAsync(int idPaciente, CancellationToken cancellationToken = default);
    Task<AntecedenteDto> InsertarAsync(int idPaciente, CreacionAntecedenteDto dto, CancellationToken cancellationToken = default);
}
