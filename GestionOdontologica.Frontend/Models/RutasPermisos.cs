using Microsoft.AspNetCore.Components;

namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Permiso que exige cada ruta del frontend. Los valores coinciden con la
/// visibilidad del menú lateral (MainLayout). Una ruta con <c>null</c> no exige
/// permiso específico: cualquier sesión iniciada puede abrirla.
/// </summary>
public static class RutasPermisos
{
    public const string MensajeAccesoDenegado = "No tienes permisos para acceder a este módulo";

    private static readonly IReadOnlyDictionary<string, string?> Mapa =
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["inicio"] = null,
            [""] = null,
            ["usuarios"] = PermisoCodigos.UsuariosVer,
            ["roles"] = PermisoCodigos.RolesVer,
            ["permisos"] = PermisoCodigos.PermisosVer,
            ["inventario"] = PermisoCodigos.InventarioVer,
            ["citas"] = PermisoCodigos.CitasVer,
            ["odontologos"] = PermisoCodigos.OdontologosVer,
            ["promociones"] = PermisoCodigos.PromocionesVer,
            ["pagos"] = PermisoCodigos.PagosVer,
            ["bitacoras"] = PermisoCodigos.BitacorasVer,
            ["pacientes"] = PermisoCodigos.PacientesVer,
            ["tratamientos"] = PermisoCodigos.TratamientosVer,
            ["expedientes"] = PermisoCodigos.PacientesExpediente,
            ["radiografias"] = null,
            ["reportes"] = PermisoCodigos.ReportesVer
        };

    /// <summary>
    /// Devuelve el permiso requerido por la página, o <c>null</c> si no exige
    /// uno específico (inicio, radiografías, páginas públicas).
    /// </summary>
    public static string? PermisoDe(Type pagina)
    {
        foreach (var atributo in pagina.GetCustomAttributes(typeof(RouteAttribute), inherit: false))
        {
            var ruta = ((RouteAttribute)atributo).Template.Trim('/');

            if (Mapa.TryGetValue(ruta, out var permiso))
            {
                return permiso;
            }
        }

        return null;
    }
}
