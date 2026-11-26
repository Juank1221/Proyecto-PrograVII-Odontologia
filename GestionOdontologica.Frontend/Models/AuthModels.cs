namespace GestionOdontologica.Frontend.Models;

/// <summary>Credenciales enviadas a POST /api/auth/login.</summary>
public sealed class LoginRequest
{
    public string Identificador { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool Recordarme { get; set; }
}

/// <summary>Datos públicos del usuario autenticado devueltos por la API.</summary>
public sealed class LoginResponse
{
    public int IdUsuario { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    /// <summary>Fecha UTC en la que la sesión local deja de ser válida.</summary>
    public DateTime ExpiraEn { get; set; }

    /// <summary>
    /// Token opaco y firmado que la API exige en Authorization: Bearer.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Códigos de permisos vigentes del rol. Solo se usan para adaptar la
    /// interfaz; el backend vuelve a validarlos en cada solicitud.
    /// </summary>
    public List<string> Permisos { get; set; } = new();
}

public sealed class RecuperarPasswordRequest
{
    public string Correo { get; set; } = string.Empty;
}

public sealed class RecuperarPasswordResponse
{
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Solo se completa en Development. Permite demostrar el flujo sin SMTP.
    /// </summary>
    public string? TokenDesarrollo { get; set; }

    public int? ExpiraEnMinutos { get; set; }
}

public sealed class RestablecerPasswordRequest
{
    public string Token { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed class MensajeResponse
{
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
/// Sesión que se conserva en localStorage. Contiene el token de API y los
/// códigos de permisos necesarios para construir la identidad de Blazor.
/// </summary>
public sealed class SesionUsuario
{
    public int IdUsuario { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public DateTime ExpiraEn { get; set; }

    public string Token { get; set; } = string.Empty;

    public List<string> Permisos { get; set; } = new();

    public bool EstaVigente => DateTime.UtcNow < ExpiraEn;

    /// <summary>Indica si la sesión incluye el permiso indicado.</summary>
    public bool TienePermiso(string codigo) =>
        Permisos.Any(permiso =>
            string.Equals(permiso, codigo, StringComparison.OrdinalIgnoreCase));

    public string Iniciales
    {
        get
        {
            var partes = NombreCompleto
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length == 0)
            {
                return "US";
            }

            return partes.Length == 1
                ? partes[0][..1].ToUpperInvariant()
                : $"{partes[0][0]}{partes[^1][0]}".ToUpperInvariant();
        }
    }

    /// <summary>Nombre del rol con la ortografía usada en la interfaz.</summary>
    public string RolLegible => Rol.Trim().ToLowerInvariant() switch
    {
        "administrador" => "Administrador",
        "odontologo" or "odontólogo" => "Odontólogo",
        "recepcionista" => "Recepcionista",
        _ => Rol
    };
}
