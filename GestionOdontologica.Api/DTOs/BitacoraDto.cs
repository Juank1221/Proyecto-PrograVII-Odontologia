namespace GestionOdontologica.Api.DTOs;

public sealed class BitacoraDto
{
    public long IdBitacora { get; init; }
    public DateTime FechaHora { get; init; }
    public string Modulo { get; init; } = string.Empty;
    public string Accion { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Resultado { get; init; } = string.Empty;
    public string ResultadoClave { get; init; } = string.Empty;
    public string Usuario { get; init; } = string.Empty;
    public string? DireccionIp { get; init; }
    public string? Detalle { get; init; }
}

public sealed class BitacoraPaginaDto
{
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int Tamano { get; init; }
    public IReadOnlyList<BitacoraDto> Items { get; init; } = [];
}

public sealed class BitacoraResumenDto
{
    public int TotalEventos { get; init; }
    public int TotalErrores { get; init; }
    public int TotalAdvertencias { get; init; }
    public int EventosHoy { get; init; }
    public DateTime? UltimoEvento { get; init; }
}

public sealed class BitacoraAccesoResumenDto
{
    public int TotalAccesos { get; init; }
    public int AccesosExitosos { get; init; }
    public int AccesosFallidos { get; init; }
    public int AccesosHoy { get; init; }
    public int DireccionesDistintas { get; init; }
    public DateTime? UltimoAcceso { get; init; }
}

public sealed class BitacoraFiltroDto
{
    public string Clave { get; init; } = string.Empty;
    public string Etiqueta { get; init; } = string.Empty;
}

public sealed class BitacoraFiltrosDto
{
    public IReadOnlyList<BitacoraFiltroDto> Modulos { get; init; } = [];
    public IReadOnlyList<BitacoraFiltroDto> Resultados { get; init; } = [];
}

public sealed class BitacoraConsultaDto
{
    public DateTime? Desde { get; init; }
    public DateTime? Hasta { get; init; }
    public string? Modulo { get; init; }
    public string? Resultado { get; init; }
    public string? Texto { get; init; }
    public int Pagina { get; init; } = 1;
    public int Tamano { get; init; } = 25;
}
