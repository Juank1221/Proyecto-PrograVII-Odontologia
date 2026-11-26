namespace GestionOdontologica.Api.Exceptions;

/// <summary>
/// Se produce cuando Oracle rechaza un INSERT/UPDATE por violar una restricción
/// UNIQUE (nombre_usuario o correo).
/// </summary>
public sealed class UsuarioDuplicadoException : Exception
{
    public UsuarioDuplicadoException(string message)
        : base(message)
    {
    }

    public UsuarioDuplicadoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
