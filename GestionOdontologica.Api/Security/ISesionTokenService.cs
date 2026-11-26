using GestionOdontologica.Api.DTOs;

namespace GestionOdontologica.Api.Security;

public sealed record SesionTokenPayload(
    int IdUsuario,
    string NombreUsuario,
    string NombreCompleto,
    string Correo,
    string Rol,
    DateTime ExpiraEnUtc);

public interface ISesionTokenService
{
    /// <summary>Genera un token opaco y firmado con los datos de la sesion.</summary>
    string GenerarToken(UsuarioDto usuario, DateTime expiraEnUtc);

    /// <summary>Valida la firma y la vigencia. Devuelve null si no es utilizable.</summary>
    SesionTokenPayload? ValidarToken(string token);
}
