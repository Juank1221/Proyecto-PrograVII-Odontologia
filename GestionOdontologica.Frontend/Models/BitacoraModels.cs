namespace GestionOdontologica.Frontend.Models;

public sealed class BitacoraDto
{
    public long IdBitacora { get; set; }
    public DateTime FechaHora { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string ResultadoClave { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string? DireccionIp { get; set; }
    public string? Detalle { get; set; }
}

public sealed class BitacoraPaginaDto
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
    public List<BitacoraDto> Items { get; set; } = new();
}

public sealed class BitacoraResumenDto
{
    public int TotalEventos { get; set; }
    public int TotalErrores { get; set; }
    public int TotalAdvertencias { get; set; }
    public int EventosHoy { get; set; }
    public DateTime? UltimoEvento { get; set; }
}

public sealed class BitacoraAccesoResumenDto
{
    public int TotalAccesos { get; set; }
    public int AccesosExitosos { get; set; }
    public int AccesosFallidos { get; set; }
    public int AccesosHoy { get; set; }
    public int DireccionesDistintas { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

public sealed class BitacoraFiltroDto
{
    public string Clave { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
}

public sealed class BitacoraFiltrosDto
{
    public List<BitacoraFiltroDto> Modulos { get; set; } = new();
    public List<BitacoraFiltroDto> Resultados { get; set; } = new();
}

public sealed class BitacoraConsultaDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Modulo { get; set; }
    public string? Resultado { get; set; }
    public string? Texto { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 25;
}
