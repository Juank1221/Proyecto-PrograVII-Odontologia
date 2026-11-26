namespace GestionOdontologica.Api.DTOs;

public sealed class PacienteDto
{
    public int IdPaciente { get; init; }
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string Identificacion { get; init; } = string.Empty;
    public DateTime? FechaNacimiento { get; init; }
    public string? Sexo { get; init; }
    public string? Telefono { get; init; }
    public string? Correo { get; init; }
    public string? Direccion { get; init; }
    public string? ContactoEmergenciaNombre { get; init; }
    public string? ContactoEmergenciaTelefono { get; init; }
    public string? ContactoEmergenciaParentesco { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime FechaModificacion { get; init; }
}
