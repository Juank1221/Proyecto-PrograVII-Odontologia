using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class ActualizacionOdontologoDto
{
    public int? IdUsuario { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden exceder los 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de colegiado es obligatorio.")]
    [StringLength(30, ErrorMessage = "El número de colegiado no puede exceder los 30 caracteres.")]
    public string NumeroColegiado { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "La especialidad no puede exceder los 80 caracteres.")]
    public string? Especialidad { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
    public string? Correo { get; set; }

    public bool? Activo { get; set; }
}
