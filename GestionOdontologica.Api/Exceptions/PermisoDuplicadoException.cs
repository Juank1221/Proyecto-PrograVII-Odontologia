namespace GestionOdontologica.Api.Exceptions;

/// <summary>
/// Se produce cuando ya existe un permiso con el mismo nombre en la tabla
/// PERMISOS (restriccion UQ_PERMISOS).
/// </summary>
public sealed class PermisoDuplicadoException : Exception
{
    public PermisoDuplicadoException(string message)
        : base(message)
    {
    }

    public PermisoDuplicadoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
