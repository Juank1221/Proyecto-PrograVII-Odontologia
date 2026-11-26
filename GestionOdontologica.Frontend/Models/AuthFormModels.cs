using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Campos de la pantalla de login. El identificador acepta tanto el nombre de
/// usuario como el correo electrónico; la contraseña no se recorta porque
/// forma parte literal de la credencial.
/// </summary>
public sealed class LoginFormModel
{
    [Required(ErrorMessage = "Ingresa tu usuario o correo electrónico.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El usuario o correo debe tener entre 3 y 150 caracteres.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Ingresa tu usuario o correo electrónico.")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [StringLength(72, MinimumLength = 1, ErrorMessage = "La contraseña no puede superar los 72 caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Extiende la sesión local de 8 horas a 7 días.</summary>
    public bool Recordarme { get; set; }
}

/// <summary>Solicitud de recuperación de contraseña por correo.</summary>
public sealed class RecuperarPasswordFormModel
{
    [Required(ErrorMessage = "Ingresa tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;
}

/// <summary>
/// Restablecimiento de contraseña con token. Se valida que ambas contraseñas
/// coincidan antes de enviar la solicitud a la API.
/// </summary>
public sealed class RestablecerPasswordFormModel : IValidatableObject
{
    [Required(ErrorMessage = "El token de recuperación es obligatorio.")]
    [StringLength(128, MinimumLength = 16, ErrorMessage = "El token de recuperación no es válido.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(72, ErrorMessage = "La contraseña no puede superar los 72 caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la nueva contraseña.")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Password))
        {
            foreach (var error in PasswordPolicy.ObtenerErrores(Password))
            {
                yield return new ValidationResult(error, new[] { nameof(Password) });
            }
        }

        if (!string.IsNullOrEmpty(Password)
            && !string.IsNullOrEmpty(ConfirmarPassword)
            && !string.Equals(Password, ConfirmarPassword, StringComparison.Ordinal))
        {
            yield return new ValidationResult(
                "Las contraseñas no coinciden.",
                new[] { nameof(ConfirmarPassword) });
        }
    }
}
