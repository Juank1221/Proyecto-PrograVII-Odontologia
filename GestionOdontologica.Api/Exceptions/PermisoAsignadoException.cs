namespace GestionOdontologica.Api.Exceptions;

/// <summary>
/// Se produce al intentar eliminar un permiso que todavia esta asignado a uno
/// o mas roles en ROL_PERMISOS.
/// </summary>
public sealed class PermisoAsignadoException : Exception
{
    public PermisoAsignadoException(string message)
        : base(message)
    {
    }
}
