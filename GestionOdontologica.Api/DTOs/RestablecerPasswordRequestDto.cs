using System.ComponentModel.DataAnnotations;
using GestionOdontologica.Api.Validation;

namespace GestionOdontologica.Api.DTOs;

/// <summary>Datos para restablecer la contraseña con un token vigente.</summary>
public sealed class RestablecerPasswordRequestDto
{
    [Required(ErrorMessage = "El token de recuperación es obligatorio.")]
    [StringLength(128, MinimumLength = 16, ErrorMessage = "El token de recuperación no es válido.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(72, ErrorMessage = "La contraseña no puede superar los 72 caracteres.")]
    [PasswordPolicy]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
