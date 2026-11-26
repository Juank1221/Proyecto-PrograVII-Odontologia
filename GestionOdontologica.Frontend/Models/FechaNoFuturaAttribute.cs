using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

/// <summary>Fechas que no pueden ser posteriores al día actual (por ejemplo, fecha de nacimiento).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class FechaNoFuturaAttribute : ValidationAttribute
{
    public FechaNoFuturaAttribute()
    {
        ErrorMessage = "La fecha no puede ser posterior a la fecha actual.";
    }

    public override bool IsValid(object? value) => value switch
    {
        null => true,
        DateTime fecha => fecha.Date <= DateTime.Today,
        DateTimeOffset fecha => fecha.Date <= DateTime.Today,
        _ => true
    };
}
