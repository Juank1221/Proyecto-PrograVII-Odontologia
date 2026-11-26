using System.ComponentModel.DataAnnotations;
using GestionOdontologica.Api.Validation;

namespace GestionOdontologica.Api.DTOs;

public sealed class CreacionOdontologoVinculadoDto
{
    public string TipoVinculacion { get; set; } = "VINCULAR";

    public int? IdUsuario { get; set; }

    [StringLength(150)]
    public string? NombreCompleto { get; set; }

    [StringLength(50)]
    public string? NombreUsuario { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? CorreoUsuario { get; set; }

    [StringLength(72, ErrorMessage = "La contraseña no puede superar los 72 caracteres.")]
    [PasswordPolicy(Obligatorio = false)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [StringLength(50)]
    public string? Rol { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de colegiado es obligatorio.")]
    [StringLength(30)]
    public string NumeroColegiado { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Especialidad { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? Correo { get; set; }
}
