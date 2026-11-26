using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class CrearTratamientoRequest
{
    [Required(ErrorMessage = "El código del tratamiento es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código no puede superar los 20 caracteres.")]
    public string CodigoTratamiento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del tratamiento es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0, 999999999999, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal Precio { get; set; }

    [Range(5, 480, ErrorMessage = "La duración debe estar entre 5 y 480 minutos.")]
    public int DuracionMinutos { get; set; } = 30;
}

public sealed class ActualizarTratamientoRequest
{
    [Required(ErrorMessage = "El código del tratamiento es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código no puede superar los 20 caracteres.")]
    public string CodigoTratamiento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del tratamiento es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0, 999999999999, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal Precio { get; set; }

    [Range(5, 480, ErrorMessage = "La duración debe estar entre 5 y 480 minutos.")]
    public int DuracionMinutos { get; set; } = 30;
}

public sealed class TratamientoDto
{
    public int IdTratamiento { get; init; }
    public string CodigoTratamiento { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public decimal Precio { get; init; }
    public int DuracionMinutos { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime FechaModificacion { get; init; }
    public bool? AsociadoActivo { get; init; }
    public DateTime? FechaAsociacion { get; init; }
}

public sealed class AsociacionTratamientoDto
{
    public int IdTratamiento { get; set; }
}
