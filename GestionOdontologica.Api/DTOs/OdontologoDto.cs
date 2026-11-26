namespace GestionOdontologica.Api.DTOs;

public sealed class OdontologoDto
{
    public int IdOdontologo { get; init; }
    public int? IdUsuario { get; init; }
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string NumeroColegiado { get; init; } = string.Empty;
    public string? Especialidad { get; init; }
    public string? Telefono { get; init; }
    public string? Correo { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime FechaModificacion { get; init; }
}
