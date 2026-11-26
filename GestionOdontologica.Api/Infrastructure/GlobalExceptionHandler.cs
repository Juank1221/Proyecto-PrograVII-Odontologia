using GestionOdontologica.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Infrastructure;

/// <summary>
/// Convierte excepciones no controladas en respuestas ProblemDetails.
/// Evita exponer detalles internos o la cadena de conexión al cliente.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        var (statusCode, title, detail) = exception switch
        {
            PasswordInvalidaException => (
                StatusCodes.Status400BadRequest,
                "Contraseña no válida",
                exception.Message),

            AdministradorProtegidoException => (
                StatusCodes.Status409Conflict,
                "Administrador protegido",
                exception.Message),

            UsuarioDuplicadoException => (
                StatusCodes.Status409Conflict,
                "Usuario duplicado",
                exception.Message),

            PacienteDuplicadoException => (
                StatusCodes.Status409Conflict,
                "Paciente duplicado",
                exception.Message),

            OdontologoDuplicadoException => (
                StatusCodes.Status409Conflict,
                "Odontólogo duplicado",
                exception.Message),

            HorarioInvalidoException => (
                StatusCodes.Status400BadRequest,
                "Horario inválido",
                exception.Message),

            PermisoDuplicadoException => (
                StatusCodes.Status409Conflict,
                "Permiso duplicado",
                exception.Message),

            PermisoAsignadoException => (
                StatusCodes.Status409Conflict,
                "Permiso asignado",
                exception.Message),

            PermisoNoDisponibleException => (
                StatusCodes.Status503ServiceUnavailable,
                "Módulo de permisos no disponible",
                exception.Message),

            OracleException { Number: >= 20000 and <= 20999 } oraEx => (
                oraEx.Number switch
                {
                    20004 => StatusCodes.Status404NotFound,
                    20003 => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                },
                oraEx.Number switch
                {
                    20004 => "Recurso no encontrado",
                    20003 => "Conflicto o duplicado",
                    _ => "Regla de negocio"
                },
                ExtraerMensajeOracle(oraEx)),

            OracleException => (
                StatusCodes.Status500InternalServerError,
                "Error de base de datos",
                "No fue posible completar la operación en la base de datos."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error inesperado al procesar la solicitud.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Error no controlado. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Solicitud rechazada con estado {StatusCode}. TraceId: {TraceId}",
                statusCode,
                httpContext.TraceIdentifier);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static string ExtraerMensajeOracle(OracleException ex)
    {
        if (string.IsNullOrWhiteSpace(ex.Message))
        {
            return "La operación no cumple una regla de negocio definida en la base de datos.";
        }

        var primeraLinea = ex.Message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        var match = System.Text.RegularExpressions.Regex.Match(primeraLinea, @"^ORA-\d+:\s*(.*)$");
        return match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value)
            ? match.Groups[1].Value.Trim()
            : primeraLinea;
    }
}
