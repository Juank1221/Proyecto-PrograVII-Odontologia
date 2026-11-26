namespace GestionOdontologica.Api.Exceptions;

/// <summary>
/// Se produce cuando la base de datos conectada no contiene las tablas del
/// modulo de permisos (por ejemplo, BD_ODONTOLOGIA_QA). La aplicacion sigue
/// funcionando con los permisos predeterminados por rol, pero la administracion
/// de permisos no esta disponible.
/// </summary>
public sealed class PermisoNoDisponibleException : Exception
{
    public PermisoNoDisponibleException(string message)
        : base(message)
    {
    }

    public PermisoNoDisponibleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
