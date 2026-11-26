namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Réplica en el cliente de las reglas de negocio sobre roles protegidos.
/// El backend vuelve a validar en cada operación.
/// </summary>
public static class RolesProtegidos
{
    public static bool EsAdministrador(string? rol)
    {
        var normalizado = (rol ?? string.Empty).Trim();

        return normalizado.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
            || normalizado.Equals("Administrador del Sistema", StringComparison.OrdinalIgnoreCase);
    }
}
