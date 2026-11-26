using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Modelo del formulario de alta y edición. Las validaciones replican las del
/// backend para dar retroalimentación inmediata. La contraseña es obligatoria
/// al crear y opcional al editar (si se deja vacía se conserva la actual).
/// </summary>
public sealed class UsuarioFormModel : IValidatableObject
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 150 caracteres.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El nombre completo es obligatorio.")]
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

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = "Odontólogo";

    /// <summary>
    /// Indica si el formulario está editando un usuario existente. No se
    /// muestra en pantalla; solo cambia las reglas de la contraseña.
    /// </summary>
    public bool EsEdicion { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Al crear la contraseña es obligatoria; al editar se puede omitir
        // para conservar la clave actual.
        if (!EsEdicion && string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult(
                "La contraseña es obligatoria.",
                new[] { nameof(Password) });
            yield break;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            yield break;
        }

        foreach (var error in PasswordPolicy.ObtenerErrores(Password))
        {
            yield return new ValidationResult(error, new[] { nameof(Password) });
        }
    }
}
