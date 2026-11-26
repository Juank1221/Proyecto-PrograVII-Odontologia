using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Data;

public enum EstadoInicioSesion
{
    Exitoso,
    CredencialesInvalidas,
    CuentaInactiva,
    CuentaBloqueada
}

/// <summary>
/// Resultado de un intento de login. <see cref="IntentosRestantes"/> solo se
/// informa cuando las credenciales son incorrectas y la cuenta aún no se
/// bloquea. <see cref="SegundosBloqueo"/> se informa al alcanzar el bloqueo
/// temporal o si ya estaba vigente.
/// </summary>
public sealed record ResultadoInicioSesion(
    EstadoInicioSesion Estado,
    UsuarioDto? Usuario = null,
    int SegundosBloqueo = 0,
    int IntentosRestantes = 0,
    int? IdUsuario = null);

/// <summary>
/// Resultado de una solicitud de recuperación. <see cref="Token"/> solo se
/// devuelve en Development para poder demostrar el flujo sin SMTP.
/// </summary>
public sealed record ResultadoSolicitudRecuperacion(bool Existe, string? Token);

public interface IAuthRepository
{
    /// <summary>
    /// Valida las credenciales, aplica la política de bloqueo temporal y, si
    /// el login es correcto, registra el acceso. El identificador puede ser
    /// el nombre de usuario o el correo electrónico.
    /// </summary>
    Task<ResultadoInicioSesion> IniciarSesionAsync(
        string identificador,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera una solicitud de recuperación para el correo indicado. Si el
    /// correo no corresponde a una cuenta activa devuelve Existe = false sin
    /// revelar esa condición al cliente.
    /// </summary>
    Task<ResultadoSolicitudRecuperacion> SolicitarRecuperacionAsync(
        string correo,
        int minutosVigencia,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restablece la contraseña a partir de un token vigente. El hash BCrypt
    /// se calcula en C# y el procedimiento solo lo persiste. Devuelve false si
    /// el token no existe, ya fue usado o expiró.
    /// </summary>
    Task<bool> RestablecerPasswordAsync(
        string token,
        string passwordHash,
        CancellationToken cancellationToken = default);
}
