namespace GestionOdontologica.Api.Exceptions;

/// <summary>
/// Se lanza cuando se intenta desactivar o degradar el rol de un usuario
/// Administrador. Es una regla de negocio explícita del sistema.
/// </summary>
public sealed class AdministradorProtegidoException : Exception
{
    public AdministradorProtegidoException(string message)
        : base(message)
    {
    }
}
