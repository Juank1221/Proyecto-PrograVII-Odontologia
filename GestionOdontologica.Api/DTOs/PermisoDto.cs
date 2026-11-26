namespace GestionOdontologica.Api.DTOs;

/// <summary>Permiso administrable del sistema (HU-20).</summary>
public sealed class PermisoDto
{
    public int IdPermiso { get; set; }

    /// <summary>Codigo estable usado por las politicas de autorizacion.</summary>
    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Modulo { get; set; } = string.Empty;

    public string Accion { get; set; } = string.Empty;

    public bool Estado { get; set; }

    public DateTime FechaCreacion { get; set; }

    /// <summary>Cantidad de roles que tienen asignado este permiso.</summary>
    public int Asignaciones { get; set; }
}
