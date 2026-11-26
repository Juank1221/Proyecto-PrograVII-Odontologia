namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Representa la información pública de un usuario.
/// La contraseña nunca se expone en las respuestas de la API.
/// </summary>
public sealed class UsuarioDto
{
    public int IdUsuario { get; init; }

    /// <summary>Columna NOMBRE_COMPLETO de la tabla USUARIOS.</summary>
    public string NombreCompleto { get; init; } = string.Empty;

    /// <summary>Columna NOMBRE_USUARIO de la tabla USUARIOS (identificador de acceso).</summary>
    public string NombreUsuario { get; init; } = string.Empty;

    public string Correo { get; init; } = string.Empty;

    /// <summary>Columna ROL de la tabla USUARIOS.</summary>
    public string Rol { get; init; } = string.Empty;

    /// <summary>Columna ACTIVO de la tabla USUARIOS (1 = activo, 0 = inactivo).</summary>
    public bool Activo { get; init; }
}
