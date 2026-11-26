using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace GestionOdontologica.Api.Notifications;

/// <summary>
/// Envío de correos por SMTP configurable. En desarrollo se puede dejar
/// <c>Email:Habilitado</c> en false: la API seguirá generando el token y, si
/// el entorno es Development, lo devolverá en la respuesta para completar el
/// flujo sin un servidor de correo.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<EmailSettings> settings,
        ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task EnviarRecuperacionPasswordAsync(
        string correoDestino,
        string token,
        int minutosVigencia,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Habilitado)
        {
            _logger.LogWarning(
                "El envío de correo está deshabilitado (Email:Habilitado=false). No se envió el enlace de recuperación a {Correo}.",
                correoDestino);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogError(
                "Email:Habilitado=true pero Email:Host está vacío. No se pudo enviar el correo a {Correo}.",
                correoDestino);
            return;
        }

        var enlace = ConstruirEnlace(token);

        using var mensaje = new MailMessage
        {
            From = new MailAddress(_settings.Remitente, _settings.NombreRemitente),
            Subject = "Restablece tu contraseña de OdontoCare",
            Body = ConstruirCuerpo(enlace, token, minutosVigencia),
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        mensaje.To.Add(correoDestino);

        using var cliente = new SmtpClient(_settings.Host, _settings.Puerto)
        {
            EnableSsl = _settings.UsarSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_settings.Usuario))
        {
            cliente.Credentials = new NetworkCredential(_settings.Usuario, _settings.Password);
        }

        await cliente.SendMailAsync(mensaje, cancellationToken);

        _logger.LogInformation(
            "Correo de recuperación enviado a {Correo}.",
            correoDestino);
    }

    private string ConstruirEnlace(string token)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_settings.FrontendBaseUrl)
            ? "http://localhost:5501"
            : _settings.FrontendBaseUrl.TrimEnd('/');

        return $"{baseUrl}/restablecer-password?token={Uri.EscapeDataString(token)}";
    }

    private static string ConstruirCuerpo(string enlace, string token, int minutosVigencia)
    {
        var minutos = minutosVigencia > 0 ? minutosVigencia : 30;

        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <body style="margin:0;padding:24px;background:#f3f7f7;font-family:Segoe UI,Arial,sans-serif;color:#102a32;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:16px;padding:28px;">
                <tr>
                  <td>
                    <h1 style="margin:0 0 12px;font-size:22px;color:#0d2c33;">Recupera tu acceso a OdontoCare</h1>
                    <p style="margin:0 0 16px;font-size:14px;line-height:1.6;color:#3e5a62;">
                      Recibimos una solicitud para restablecer la contraseña de tu cuenta.
                      Este enlace es de un solo uso y expira en {minutos} minutos.
                    </p>
                    <p style="margin:0 0 20px;">
                      <a href="{enlace}" style="display:inline-block;padding:12px 20px;border-radius:10px;background:#0f766e;color:#ffffff;text-decoration:none;font-weight:600;">
                        Restablecer contraseña
                      </a>
                    </p>
                    <p style="margin:0 0 8px;font-size:12px;color:#70858b;">Si el botón no funciona, copia este código en la pantalla de restablecimiento:</p>
                    <p style="margin:0 0 20px;padding:10px;background:#f3faf8;border:1px dashed #cde3df;border-radius:8px;font-family:Consolas,monospace;font-size:12px;word-break:break-all;color:#144d49;">{WebUtility.HtmlEncode(token)}</p>
                    <p style="margin:0;font-size:12px;line-height:1.5;color:#70858b;">
                      Si no solicitaste este cambio, ignora este correo. Tu contraseña actual seguirá funcionando.
                    </p>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
