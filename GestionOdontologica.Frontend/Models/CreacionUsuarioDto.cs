namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Cuerpo del POST /api/usuarios. El backend hashea la contraseña con BCrypt
/// y persiste el rol en la tabla USUARIOS.
/// </summary>
public sealed class CreacionUsuarioDto
{
    public string NombreCompleto { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;
}
