namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Estado y utilidades de paginación en memoria para las tablas del sistema.
/// Los listados se cargan completos desde la API y se paginan en el cliente
/// para mantener la navegación inmediata sin consultas adicionales.
/// </summary>
public sealed class Paginacion
{
    public static readonly IReadOnlyList<int> OpcionesTamano = new[] { 10, 25, 50 };

    /// <summary>Página actual (1-based). Se ajusta al rango válido al paginar.</summary>
    public int Pagina { get; private set; } = 1;

    /// <summary>Cantidad de filas por página.</summary>
    public int Tamano { get; private set; } = OpcionesTamano[0];

    public int TotalPaginas(int totalRegistros)
        => Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)Tamano));

    /// <summary>
    /// Página válida para el total actual. Evita mostrar una página vacía
    /// cuando los filtros reducen el conjunto de datos.
    /// </summary>
    public int PaginaEfectiva(int totalRegistros)
        => Math.Clamp(Pagina, 1, TotalPaginas(totalRegistros));

    /// <summary>Devuelve solo las filas que corresponden a la página actual.</summary>
    public IReadOnlyList<T> Aplicar<T>(IEnumerable<T> origen)
    {
        var lista = origen as IReadOnlyList<T> ?? origen.ToList();
        var pagina = PaginaEfectiva(lista.Count);
        return lista.Skip((pagina - 1) * Tamano).Take(Tamano).ToList();
    }

    public void IrA(int pagina, int totalRegistros)
        => Pagina = Math.Clamp(pagina, 1, TotalPaginas(totalRegistros));

    public void CambiarTamano(int tamano)
    {
        Tamano = OpcionesTamano.Contains(tamano) ? tamano : OpcionesTamano[0];
        Pagina = 1;
    }

    /// <summary>Vuelve a la primera página; se usa al cambiar búsqueda o filtros.</summary>
    public void Reiniciar() => Pagina = 1;
}
