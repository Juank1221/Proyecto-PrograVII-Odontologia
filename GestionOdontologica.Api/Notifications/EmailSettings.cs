namespace GestionOdontologica.Api.Notifications;

/// <summary>
/// Configuración del servicio de correo. Se enlaza desde la sección "Email"
/// de appsettings.json. Si <see cref="Habilitado"/> es false, el sistema no
/// intenta enviar correos y registra un aviso en el log (útil en desarrollo).
/// </summary>
public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public bool Habilitado { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Puerto { get; set; } = 587;

    public bool UsarSsl { get; set; } = true;

    public string Usuario { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Remitente { get; set; } = "no-reply@odontocare.local";

    public string NombreRemitente { get; set; } = "OdontoCare";

    /// <summary>
    /// URL pública del frontend para construir el enlace de restablecimiento.
    /// </summary>
    public string FrontendBaseUrl { get; set; } = "http://localhost:5501";
}
