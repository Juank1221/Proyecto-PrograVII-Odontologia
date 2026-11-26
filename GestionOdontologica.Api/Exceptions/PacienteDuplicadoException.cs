namespace GestionOdontologica.Api.Exceptions;

public sealed class PacienteDuplicadoException : Exception
{
    public PacienteDuplicadoException(string message) : base(message)
    {
    }
}
