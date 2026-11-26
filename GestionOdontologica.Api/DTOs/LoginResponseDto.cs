namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Datos públicos del usuario autenticado. Nunca incluye la contraseña ni el
/// hash. La sesión se conserva en el cliente hasta <see cref="ExpiraEn"/>.
/// </summary>
public sealed class LoginResponseDto
{
    public int IdUsuario { get; init; }

    public string NombreCompleto { get; init; } = string.Empty;

    public string NombreUsuario { get; init; } = string.Empty;

    public string Correo { get; init; } = string.Empty;

    public string Rol { get; init; } = string.Empty;

    /// <summary>Fecha UTC en la que el cliente debe descartar la sesión.</summary>
    public DateTime ExpiraEn { get; init; }

    /// <summary>
    /// Token opaco y firmado que el cliente debe enviar en
    /// <c>Authorization: Bearer</c> en cada solicitud autenticada.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Códigos de permisos vigentes del rol. El frontend los usa para ocultar
    /// acciones; el backend los vuelve a validar en cada solicitud.
    /// </summary>
    public IReadOnlyList<string> Permisos { get; init; } = Array.Empty<string>();
}
