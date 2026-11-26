using System.Text;
using System.Text.Json;
using GestionOdontologica.Api.DTOs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace GestionOdontologica.Api.Security;

/// <summary>
/// Token de sesion firmado con DataProtection. No es un JWT: el contenido va
/// cifrado y autenticado, por lo que el cliente no puede modificarlo. Los
/// permisos no se guardan en el token; se consultan en cada solicitud para que
/// los cambios de asignacion tengan efecto inmediato.
/// </summary>
public sealed class SesionTokenService : ISesionTokenService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DateTime TiempoMaximo = new(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    private readonly IDataProtector _protector;

    public SesionTokenService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(
            "GestionOdontologica.Sesion.v1");
    }

    public string GenerarToken(UsuarioDto usuario, DateTime expiraEnUtc)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var expira = expiraEnUtc.Kind == DateTimeKind.Utc
            ? expiraEnUtc
            : expiraEnUtc.ToUniversalTime();

        if (expira > TiempoMaximo)
        {
            expira = TiempoMaximo;
        }

        var payload = new SesionTokenPayload(
            usuario.IdUsuario,
            usuario.NombreUsuario,
            usuario.NombreCompleto,
            usuario.Correo,
            usuario.Rol,
            expira);

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var protegido = _protector.Protect(Encoding.UTF8.GetBytes(json));

        return WebEncoders.Base64UrlEncode(protegido);
    }

    public SesionTokenPayload? ValidarToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var bytes = WebEncoders.Base64UrlDecode(token.Trim());
            var json = Encoding.UTF8.GetString(_protector.Unprotect(bytes));
            var payload = JsonSerializer.Deserialize<SesionTokenPayload>(json, JsonOptions);

            if (payload is null
                || payload.IdUsuario <= 0
                || string.IsNullOrWhiteSpace(payload.Rol)
                || payload.ExpiraEnUtc <= DateTime.UtcNow)
            {
                return null;
            }

            return payload;
        }
        catch (Exception)
        {
            // Firma invalida, token manipulado, base64 incorrecto o JSON roto.
            return null;
        }
    }
}
