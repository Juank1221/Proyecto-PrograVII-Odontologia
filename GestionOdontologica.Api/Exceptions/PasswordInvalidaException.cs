namespace GestionOdontologica.Api.Exceptions;

/// <summary>
/// Se lanza cuando una contraseña no cumple la política de complejidad.
/// </summary>
public sealed class PasswordInvalidaException : Exception
{
    public PasswordInvalidaException(string message)
        : base(message)
    {
    }
}
