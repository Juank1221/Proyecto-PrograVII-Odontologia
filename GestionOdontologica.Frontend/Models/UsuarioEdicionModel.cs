using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Modelo de vista para el formulario/modal de edición de usuario.
/// </summary>
public sealed class UsuarioEdicionModel : IValidatableObject
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 150 caracteres.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El nombre completo no puede contener solo espacios.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = "Odontólogo";

    public bool Activo { get; set; } = true;

    [StringLength(72, ErrorMessage = "La nueva contraseña no puede superar los 72 caracteres.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            yield break;
        }

        foreach (var error in PasswordPolicy.ObtenerErrores(Password))
        {
            yield return new ValidationResult(error, new[] { nameof(Password) });
        }
    }

    public static UsuarioEdicionModel DesdeUsuario(UsuarioDto usuario)
    {
        return new UsuarioEdicionModel
        {
            IdUsuario = usuario.IdUsuario,
            NombreCompleto = usuario.NombreCompleto,
            NombreUsuario = usuario.NombreUsuario,
            Correo = usuario.Correo,
            Rol = string.IsNullOrWhiteSpace(usuario.Rol) ? "Odontólogo" : usuario.Rol,
            Activo = usuario.Activo,
            Password = null
        };
    }

    public ActualizacionUsuarioDto AActualizacionDto()
    {
        return new ActualizacionUsuarioDto
        {
            NombreCompleto = NombreCompleto.Trim(),
            NombreUsuario = NombreUsuario.Trim(),
            Correo = Correo.Trim(),
            Rol = Rol.Trim(),
            Activo = Activo,
            Password = string.IsNullOrWhiteSpace(Password) ? null : Password.Trim()
        };
    }
}
