namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Modelo para enviar actualizaciones a la API (PUT /api/usuarios/{id}).
/// </summary>
public sealed class ActualizacionUsuarioDto
{
    public string NombreCompleto { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string? Rol { get; set; }

    public bool? Activo { get; set; }

    public string? Password { get; set; }
}
