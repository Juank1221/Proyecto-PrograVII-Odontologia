namespace GestionOdontologica.Api.Exceptions;

public sealed class OdontologoDuplicadoException : Exception
{
    public OdontologoDuplicadoException(string message) : base(message)
    {
    }
}
