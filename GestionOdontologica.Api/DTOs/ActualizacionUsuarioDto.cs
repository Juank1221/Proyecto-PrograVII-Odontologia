using System.ComponentModel.DataAnnotations;
using GestionOdontologica.Api.Validation;

namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Datos para actualizar un usuario existente.
/// Si <see cref="Activo"/> o <see cref="Rol"/> se omiten, el repositorio conserva
/// el valor actual del usuario. Si <see cref="Password"/> se omite o está en blanco,
/// se conserva la contraseña actual sin modificar.
/// </summary>
public sealed class ActualizacionUsuarioDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [StringLength(50, MinimumLength = 3, ErrorMessage = "El rol debe tener entre 3 y 50 caracteres.")]
    public string? Rol { get; set; }

    /// <summary>
    /// Estado del usuario. Es opcional para permitir actualizar los datos sin
    /// reactivar accidentalmente una cuenta desactivada.
    /// </summary>
    public bool? Activo { get; set; }

    /// <summary>
    /// Nueva contraseña opcional. Si se envía, debe cumplir la política de
    /// complejidad. Si es nula o vacía, se conserva la contraseña actual.
    /// </summary>
    [StringLength(72, ErrorMessage = "La contraseña no puede superar los 72 caracteres.")]
    [PasswordPolicy(Obligatorio = false)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
