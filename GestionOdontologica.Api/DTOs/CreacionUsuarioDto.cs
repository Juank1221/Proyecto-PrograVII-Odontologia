using System.ComponentModel.DataAnnotations;
using GestionOdontologica.Api.Validation;

namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Datos requeridos para crear un usuario.
/// La contraseña se recibe en texto plano únicamente para ser hasheada en el backend.
/// </summary>
public sealed class CreacionUsuarioDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 150 caracteres.")]
    [NombrePersona]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    // BCrypt trunca la contraseña a 72 bytes; se limita a 72 caracteres para evitar
    // que dos contraseñas largas diferentes terminen generando el mismo hash.
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(72, ErrorMessage = "La contraseña no puede superar los 72 caracteres.")]
    [PasswordPolicy]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El rol debe tener entre 3 y 50 caracteres.")]
    public string Rol { get; set; } = string.Empty;
}
