
using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Frontend.Models;

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

    [Range(typeof(decimal), "0", "99999999.99",
        ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal Precio { get; set; }
}
