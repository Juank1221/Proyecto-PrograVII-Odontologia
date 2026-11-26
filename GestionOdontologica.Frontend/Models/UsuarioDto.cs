namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Usuario devuelto por la Web API. Coincide con UsuarioDto del backend.
/// </summary>
public sealed class UsuarioDto
{
    public int IdUsuario { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
