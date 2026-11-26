using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class ActualizacionPacienteDto
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden exceder los 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [StringLength(20, ErrorMessage = "La identificación no puede exceder los 20 caracteres.")]
    public string Identificacion { get; set; } = string.Empty;

    public DateTime? FechaNacimiento { get; set; }

    [StringLength(10, ErrorMessage = "El sexo no puede exceder los 10 caracteres.")]
    public string? Sexo { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
    public string? Correo { get; set; }

    [StringLength(250, ErrorMessage = "La dirección no puede exceder los 250 caracteres.")]
    public string? Direccion { get; set; }

    public bool? Activo { get; set; }
}
