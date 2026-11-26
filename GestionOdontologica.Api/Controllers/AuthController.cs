using System.Globalization;
using System.Net;
using System.Security.Claims;
using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Notifications;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOdontologica.Api.Controllers;

/// <summary>
/// Endpoints públicos de autenticación y recuperación de contraseña.
/// El login emite un token opaco firmado con DataProtection; el cliente lo
/// envía en Authorization: Bearer para acceder al resto de la API.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private const int HorasSesion = 8;
    private const int DiasSesionRecordada = 7;
    private const int MinutosVigenciaToken = 30;

    private readonly IAuthRepository _authRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IBitacoraRepository _bitacoraRepository;
    private readonly ISesionTokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthRepository authRepository,
        IPermisoRepository permisoRepository,
        IBitacoraRepository bitacoraRepository,
        ISesionTokenService tokenService,
        IEmailSender emailSender,
        IWebHostEnvironment environment,
        ILogger<AuthController> logger)
    {
        _authRepository = authRepository;
        _permisoRepository = permisoRepository;
        _bitacoraRepository = bitacoraRepository;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Valida las credenciales y devuelve los datos públicos del usuario.
    /// 401 credenciales incorrectas, 403 cuenta desactivada y 423 bloqueo temporal.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status423Locked)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponseDto>> IniciarSesion(
        [FromBody] LoginRequestDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _authRepository.IniciarSesionAsync(
            dto.Identificador,
            dto.Password,
            cancellationToken);

        var direccionIp = ObtenerDireccionIp();

        switch (resultado.Estado)
        {
            case EstadoInicioSesion.Exitoso when resultado.Usuario is { } usuario:
                await RegistrarAccesoAsync(
                    usuario.IdUsuario,
                    "LOGIN",
                    "EXITO",
                    direccionIp,
                    "Inicio de sesión correcto",
                    cancellationToken);

                _logger.LogInformation(
                    "Inicio de sesión exitoso para el usuario {NombreUsuario}.",
                    usuario.NombreUsuario);

                var duracion = dto.Recordarme
                    ? TimeSpan.FromDays(DiasSesionRecordada)
                    : TimeSpan.FromHours(HorasSesion);

                var expiraEn = DateTime.UtcNow.Add(duracion);

                var permisos = await _permisoRepository.ObtenerCodigosPorNombreRolAsync(
                    usuario.Rol,
                    cancellationToken);

                var token = _tokenService.GenerarToken(usuario, expiraEn);

                return Ok(new LoginResponseDto
                {
                    IdUsuario = usuario.IdUsuario,
                    NombreCompleto = usuario.NombreCompleto,
                    NombreUsuario = usuario.NombreUsuario,
                    Correo = usuario.Correo,
                    Rol = usuario.Rol,
                    ExpiraEn = expiraEn,
                    Token = token,
                    Permisos = permisos
                });

            case EstadoInicioSesion.CuentaInactiva:
                await RegistrarAccesoAsync(
                    resultado.IdUsuario,
                    "LOGIN",
                    "ERROR",
                    direccionIp,
                    "Intento de acceso a una cuenta desactivada",
                    cancellationToken);

                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Cuenta desactivada",
                    detail: "Tu cuenta está desactivada. Contacta con el administrador del sistema.");

            case EstadoInicioSesion.CuentaBloqueada:
                await RegistrarAccesoAsync(
                    resultado.IdUsuario,
                    "LOGIN",
                    "ERROR",
                    direccionIp,
                    "Intento de acceso a una cuenta bloqueada temporalmente",
                    cancellationToken);

                // SEGUNDOS_BLOQUEO es un valor redondeado hacia arriba por Oracle;
                // se redondea al minuto más cercano para no mostrar 16 minutos
                // en un bloqueo de 15.
                var minutos = Math.Max(
                    1,
                    (int)Math.Round(resultado.SegundosBloqueo / 60d, MidpointRounding.AwayFromZero));

                Response.Headers.RetryAfter = resultado.SegundosBloqueo.ToString();

                return Problem(
                    statusCode: StatusCodes.Status423Locked,
                    title: "Cuenta bloqueada temporalmente",
                    detail: $"La cuenta está bloqueada por intentos fallidos. Inténtalo de nuevo en {minutos} minuto{(minutos == 1 ? string.Empty : "s")} o restablece tu contraseña.");

            default:
                await RegistrarAccesoAsync(
                    resultado.IdUsuario,
                    "LOGIN",
                    "ERROR",
                    direccionIp,
                    "Intento de inicio de sesión con credenciales inválidas",
                    cancellationToken);

                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Credenciales incorrectas",
                    detail: "El usuario o la contraseña no son correctos.");
        }
    }

    /// <summary>
    /// Registra el cierre de sesión del usuario autenticado. El token es
    /// stateless; este endpoint solo deja la traza de auditoría con la IP.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CerrarSesion(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(idClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idUsuario))
        {
            await RegistrarAccesoAsync(
                idUsuario,
                "LOGOUT",
                "EXITO",
                ObtenerDireccionIp(),
                "Cierre de sesión",
                cancellationToken);
        }

        return NoContent();
    }

    /// <summary>
    /// Genera un token de recuperación. La respuesta es siempre la misma para
    /// no revelar si el correo existe. En Development se incluye el token para
    /// poder completar el flujo sin un servidor de correo.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("recuperar-password")]
    [ProducesResponseType(typeof(RecuperarPasswordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RecuperarPasswordResponseDto>> RecuperarPassword(
        [FromBody] RecuperarPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _authRepository.SolicitarRecuperacionAsync(
            dto.Correo,
            MinutosVigenciaToken,
            cancellationToken);

        var tokenDesarrollo = _environment.IsDevelopment() && resultado.Existe
            ? resultado.Token
            : null;

        if (resultado.Existe)
        {
            _logger.LogInformation(
                "Solicitud de recuperación registrada para el correo {Correo}.",
                dto.Correo.Trim());

            if (!string.IsNullOrWhiteSpace(resultado.Token))
            {
                try
                {
                    await _emailSender.EnviarRecuperacionPasswordAsync(
                        dto.Correo.Trim(),
                        resultado.Token,
                        MinutosVigenciaToken,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    // Un fallo de SMTP no debe revelar si el correo existe ni
                    // romper la respuesta genérica de recuperación.
                    _logger.LogError(
                        ex,
                        "No se pudo enviar el correo de recuperación a {Correo}.",
                        dto.Correo.Trim());
                }
            }
        }

        return Ok(new RecuperarPasswordResponseDto
        {
            Mensaje = "Si el correo pertenece a una cuenta activa, recibirás las instrucciones para restablecer tu contraseña.",
            TokenDesarrollo = tokenDesarrollo,
            ExpiraEnMinutos = tokenDesarrollo is null ? null : MinutosVigenciaToken
        });
    }

    /// <summary>
    /// Restablece la contraseña con un token vigente y de un solo uso.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("restablecer-password")]
    [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MensajeResponseDto>> RestablecerPassword(
        [FromBody] RestablecerPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        var passwordHash = PasswordHasher.Hash(dto.Password);

        var actualizado = await _authRepository.RestablecerPasswordAsync(
            dto.Token,
            passwordHash,
            cancellationToken);

        if (!actualizado)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Token inválido",
                detail: "El enlace de recuperación no es válido o ya expiró. Solicita uno nuevo.");
        }

        _logger.LogInformation("Contraseña restablecida mediante token de recuperación.");

        return Ok(new MensajeResponseDto
        {
            Mensaje = "Tu contraseña se actualizó correctamente. Ya puedes iniciar sesión."
        });
    }

    private async Task RegistrarAccesoAsync(
        int? idUsuario,
        string accion,
        string resultado,
        string? direccionIp,
        string detalle,
        CancellationToken cancellationToken)
    {
        try
        {
            await _bitacoraRepository.RegistrarAccesoAsync(
                idUsuario,
                accion,
                resultado,
                direccionIp,
                detalle,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // La bitácora es complementaria: un fallo de auditoría no debe
            // impedir el inicio ni el cierre de sesión.
            _logger.LogWarning(ex, "No se pudo registrar el evento {Accion} en la bitácora.", accion);
        }
    }

    /// <summary>
    /// Obtiene la IP de origen normalizada. Respeta X-Forwarded-For cuando la
    /// API está detrás de un proxy y convierte direcciones IPv4 mapeadas en IPv6
    /// a su forma IPv4 legible.
    /// </summary>
    private string? ObtenerDireccionIp()
    {
        var reenviada = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var valor = !string.IsNullOrWhiteSpace(reenviada)
            ? reenviada.Split(',').FirstOrDefault()
            : HttpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        valor = valor.Trim();

        if (IPAddress.TryParse(valor, out var direccion))
        {
            if (direccion.IsIPv4MappedToIPv6)
            {
                direccion = direccion.MapToIPv4();
            }

            valor = direccion.ToString();
        }

        var zona = valor.IndexOf('%');
        if (zona >= 0)
        {
            valor = valor[..zona];
        }

        return valor.Length > 45 ? valor[..45] : valor;
    }
}
