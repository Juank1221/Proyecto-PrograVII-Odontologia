namespace GestionOdontologica.Frontend.Models;

public sealed class OdontologoDto
{
    public int IdOdontologo { get; set; }
    public int? IdUsuario { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NumeroColegiado { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}

public sealed class CreacionOdontologoDto
{
    public int? IdUsuario { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NumeroColegiado { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
}

public sealed class ActualizacionOdontologoDto
{
    public int? IdUsuario { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NumeroColegiado { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public bool? Activo { get; set; }
}

public sealed class CreacionOdontologoVinculadoDto
{
    public string TipoVinculacion { get; set; } = "VINCULAR";
    public int? IdUsuario { get; set; }
    public string? NombreCompleto { get; set; }
    public string? NombreUsuario { get; set; }
    public string? CorreoUsuario { get; set; }
    public string? Password { get; set; }
    public string? Rol { get; set; } = "ODONTOLOGO";
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NumeroColegiado { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
}
