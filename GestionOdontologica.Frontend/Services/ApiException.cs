using GestionOdontologica.Frontend.Models;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Error enriquecido con el status HTTP y el ProblemDetails devuelto por la API.
/// Status 0 significa que la API no fue alcanzable (red, CORS o timeout).
/// </summary>
public sealed class ApiException : Exception
{
    public ApiException(string message, int status = 0, ProblemDetailsDto? problem = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Status = status;
        Problem = problem;
    }

    public int Status { get; }

    public ProblemDetailsDto? Problem { get; }
}
