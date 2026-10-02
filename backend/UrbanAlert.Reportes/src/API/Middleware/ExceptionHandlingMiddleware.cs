using System.Net;
using Domain.Reportes;
using Microsoft.EntityFrameworkCore;

namespace API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (ex is TransicionEstadoInvalidaException or ArgumentException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Conflicto de concurrencia al actualizar un reporte.");
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsJsonAsync(new { message = "El reporte fue modificado por otra solicitud. Vuelve a intentarlo." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado al procesar la solicitud {Method} {Path}.", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Ocurrió un error inesperado. Intenta nuevamente más tarde." });
        }
    }
}
