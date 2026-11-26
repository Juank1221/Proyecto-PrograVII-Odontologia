namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Representa el ProblemDetails (RFC 7807) que devuelve la Web API en los errores.
/// </summary>
public sealed class ProblemDetailsDto
{
    public string? Type { get; set; }

    public string? Title { get; set; }

    public int? Status { get; set; }

    public string? Detail { get; set; }

    public string? Instance { get; set; }

    public Dictionary<string, string[]>? Errors { get; set; }
}
