namespace GestionOdontologica.Api.DTOs;

/// <summary>
/// Respuesta genérica de recuperación. <see cref="TokenDesarrollo"/> solo se
/// completa en el entorno Development y cuando el correo pertenece a una
/// cuenta activa; permite demostrar el flujo sin servidor SMTP.
/// </summary>
public sealed class RecuperarPasswordResponseDto
{
    public string Mensaje { get; init; } = string.Empty;

    public string? TokenDesarrollo { get; init; }

    public int? ExpiraEnMinutos { get; init; }
}
