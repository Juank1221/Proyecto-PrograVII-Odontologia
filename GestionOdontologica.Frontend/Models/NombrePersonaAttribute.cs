using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

/// <summary>Nombres y apellidos de personas: solo letras, espacios, apostrofes, guiones y puntos.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NombrePersonaAttribute : RegularExpressionAttribute
{
    public const string Patron = @"^\s*\p{L}+(?:[\s'.\-]+\p{L}+)*\.?\s*$";

    public NombrePersonaAttribute() : base(Patron)
    {
        ErrorMessage = "Solo puede contener letras, espacios, apostrofes, guiones y puntos; no se permiten numeros.";
    }
}