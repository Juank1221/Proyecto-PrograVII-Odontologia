using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Credenciales enviadas por la pantalla de login. El identificador puede ser
/// el nombre de usuario o el correo electrónico.
/// </summary>
public sealed class LoginRequestDto
{
    [Required(ErrorMessage = "Ingresa tu usuario o correo electrónico.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El usuario o correo debe tener entre 3 y 150 caracteres.")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [StringLength(72, MinimumLength = 1, ErrorMessage = "La contraseña no puede superar los 72 caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Si es true, la sesión del cliente se conserva por 7 días en lugar de 8 horas.
    /// </summary>
    public bool Recordarme { get; set; }
}
