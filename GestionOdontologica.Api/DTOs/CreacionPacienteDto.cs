using GestionOdontologica.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class CreacionPacienteDto
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden exceder los 100 caracteres.")]
    [NombrePersona]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
    [NombrePersona]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [StringLength(20, ErrorMessage = "La identificación no puede exceder los 20 caracteres.")]
    public string Identificacion { get; set; } = string.Empty;

    [FechaNoFutura(ErrorMessage = "La fecha de nacimiento no puede ser posterior a la fecha actual.")]
    public DateTime? FechaNacimiento { get; set; }

    [StringLength(10, ErrorMessage = "El género no puede exceder los 10 caracteres.")]
    public string? Sexo { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
    public string? Correo { get; set; }

    [StringLength(250, ErrorMessage = "La dirección no puede exceder los 250 caracteres.")]
    public string? Direccion { get; set; }

    [StringLength(150, ErrorMessage = "El nombre del contacto no puede exceder los 150 caracteres.")]
    [NombrePersona]
    public string? ContactoEmergenciaNombre { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono del contacto no puede exceder los 20 caracteres.")]
    [RegularExpression(@"^[0-9+\-\s()]*$", ErrorMessage = "El teléfono del contacto solo puede contener números, espacios, +, -, ( y ).")]
    public string? ContactoEmergenciaTelefono { get; set; }

    [StringLength(50, ErrorMessage = "El parentesco no puede exceder los 50 caracteres.")]
    [NombrePersona]
    public string? ContactoEmergenciaParentesco { get; set; }
}
