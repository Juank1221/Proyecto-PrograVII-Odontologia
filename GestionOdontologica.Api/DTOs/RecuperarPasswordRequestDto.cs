using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

/// <summary>Solicitud de recuperación de contraseña por correo.</summary>
public sealed class RecuperarPasswordRequestDto
{
    [Required(ErrorMessage = "Ingresa tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;
}
