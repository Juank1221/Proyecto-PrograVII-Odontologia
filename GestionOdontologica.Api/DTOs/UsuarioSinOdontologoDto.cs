namespace GestionOdontologica.Api.DTOs;

public sealed class UsuarioSinOdontologoDto
{
    public int IdUsuario { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string NombreUsuario { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    public bool Activo { get; init; }
}
