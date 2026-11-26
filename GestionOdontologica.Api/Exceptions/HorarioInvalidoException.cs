namespace GestionOdontologica.Api.Exceptions;

/// <summary>
/// Se lanza cuando la configuración de horarios de un odontólogo contiene
/// rangos inválidos o turnos que se traslapan en un mismo día.
/// </summary>
public sealed class HorarioInvalidoException : Exception
{
    public HorarioInvalidoException(string message) : base(message)
    {
    }
}
