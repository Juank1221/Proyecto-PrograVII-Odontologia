namespace GestionOdontologica.Api.Security;

/// <summary>
/// Reglas de negocio sobre roles que no pueden quedar sin acceso al sistema.
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
