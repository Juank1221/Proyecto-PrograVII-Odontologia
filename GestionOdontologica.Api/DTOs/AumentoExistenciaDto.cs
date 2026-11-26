using System.ComponentModel.DataAnnotations;

namespace GestionOdontologica.Api.DTOs;

public sealed class AumentoExistenciaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor que cero.")]
    public int IdProducto { get; set; }

    [Range(1L, 9999999999L, ErrorMessage = "La cantidad debe ser un número entero mayor que cero.")]
    public long Cantidad { get; set; }
}
