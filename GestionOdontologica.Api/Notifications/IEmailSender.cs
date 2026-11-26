namespace GestionOdontologica.Api.Notifications;

/// <summary>
/// Abstracción del envío de correos. Permite sustituir SMTP por un proveedor
/// transaccional sin tocar el controlador de autenticación.
/// </summary>
public interface IEmailSender
{
    Task EnviarRecuperacionPasswordAsync(
        string correoDestino,
        string token,
        int minutosVigencia,
        CancellationToken cancellationToken = default);
}
