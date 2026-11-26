using System.Globalization;

namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Vista del usuario para la tabla. El rol viene de la API
/// (columna ROL de la tabla USUARIOS).
/// </summary>
public sealed record UsuarioVista(UsuarioDto Usuario)
{
    public string Nombre => !string.IsNullOrWhiteSpace(Usuario.NombreCompleto)
        ? Usuario.NombreCompleto
        : !string.IsNullOrWhiteSpace(Usuario.NombreUsuario)
            ? Usuario.NombreUsuario
            : "Usuario sin nombre";

    public string Iniciales => ObtenerIniciales(Usuario.NombreCompleto, Usuario.NombreUsuario);

    public string Rol => string.IsNullOrWhiteSpace(Usuario.Rol) ? "Sin rol" : Usuario.Rol;

    public bool Activo => Usuario.Activo;

    /// <summary>
    /// El usuario Administrador está protegido por una regla de negocio: no
    /// puede desactivarse ni perder su rol.
    /// </summary>
    public bool EsAdministrador => RolesProtegidos.EsAdministrador(Usuario.Rol);

    public string ClaseFila => Activo ? string.Empty : "user-row--inactive";

    public string ClaseTono
    {
        get
        {
            var tono = (Math.Abs(Usuario.IdUsuario % 4)) + 1;
            return $"avatar--tone-{tono}";
        }
    }

    public string ClaseRol => Rol switch
    {
        "Administrador" => "role-badge--admin",
        "Odontólogo" => "role-badge--dentist",
        "Recepcionista" => "role-badge--reception",
        _ => "role-badge--default"
    };

    public string Meta
    {
        get
        {
            var partes = new List<string>(2);
            var usuario = (Usuario.NombreUsuario ?? string.Empty).Trim();
            var correo = (Usuario.Correo ?? string.Empty).Trim();

            if (usuario.Length > 0)
            {
                partes.Add($"@{usuario}");
            }

            if (correo.Length > 0)
            {
                partes.Add(correo);
            }

            return string.Join(" · ", partes);
        }
    }

    public static string ObtenerIniciales(string? nombreCompleto, string? nombreUsuario = null)
    {
        var fuente = (nombreCompleto ?? string.Empty).Trim();

        if (fuente.Length == 0)
        {
            fuente = (nombreUsuario ?? string.Empty).Trim();
        }

        if (fuente.Length == 0)
        {
            return "US";
        }

        var partes = fuente.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length >= 2)
        {
            return $"{partes[0][0]}{partes[1][0]}".ToUpper(CultureInfo.CurrentCulture);
        }

        return fuente.Length >= 2
            ? fuente[..2].ToUpper(CultureInfo.CurrentCulture)
            : fuente.ToUpper(CultureInfo.CurrentCulture);
    }
}
