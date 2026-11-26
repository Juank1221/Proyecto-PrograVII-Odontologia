using System.ComponentModel.DataAnnotations;
using GestionOdontologica.Api.Security;

namespace GestionOdontologica.Api.Validation;

/// <summary>
/// Valida una contraseña con <see cref="PasswordPolicy"/>. Se puede marcar como
/// opcional para campos de edición donde dejar el valor vacío conserva la
/// contraseña actual.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class PasswordPolicyAttribute : ValidationAttribute
{
    /// <summary>
    /// Cuando es <c>false</c>, un valor nulo o vacío se considera válido
    /// (campo opcional). Por defecto la contraseña es obligatoria.
    /// </summary>
    public bool Obligatorio { get; set; } = true;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;

        if (string.IsNullOrWhiteSpace(password))
        {
            return Obligatorio
                ? new ValidationResult(
                    "La contraseña es obligatoria.",
                    new[] { validationContext.MemberName ?? string.Empty })
                : ValidationResult.Success;
        }

        var errores = PasswordPolicy.ObtenerErrores(password);

        return errores.Count == 0
            ? ValidationResult.Success
            : new ValidationResult(
                string.Join(" ", errores),
                new[] { validationContext.MemberName ?? string.Empty });
    }
}
