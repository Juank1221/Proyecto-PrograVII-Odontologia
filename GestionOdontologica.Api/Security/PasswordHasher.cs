using GestionOdontologica.Api.Exceptions;

namespace GestionOdontologica.Api.Security;

/// <summary>
/// Único punto donde se calculan y verifican los hashes BCrypt. Oracle no
/// ofrece BCrypt de forma nativa, por lo que la API entrega el hash a los
/// procedimientos almacenados y estos solo lo persisten. Antes de hashear se
/// aplica la política de contraseñas para que ninguna ruta de la API pueda
/// guardar una clave débil.
/// </summary>
public static class PasswordHasher
{
    // BCrypt trunca a 72 bytes; el trabajo 12 mantiene un costo razonable en
    // el servidor sin afectar la experiencia de login.
    private const int WorkFactor = 12;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var errores = PasswordPolicy.ObtenerErrores(password);

        if (errores.Count > 0)
        {
            throw new PasswordInvalidaException(string.Join(" ", errores));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public static bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Un hash almacenado con formato inválido no debe romper el login.
            return false;
        }
    }
}
