using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Auth;
using Ticketing.Api.Tickets;

namespace Ticketing.Api.Common;

public sealed class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = Map(exception, context);

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            }
        });
    }

    private static (int Status, string Title, string Detail) Map(
        Exception exception, HttpContext context) => exception switch
    {
        TicketNotFoundException ex =>
            (StatusCodes.Status404NotFound, "Ticket not found", ex.Message),

        InvalidTicketTransitionException ex =>
            (StatusCodes.Status409Conflict, "Invalid status transition", ex.Message),

        DbUpdateConcurrencyException =>
            (StatusCodes.Status409Conflict, "Concurrent modification",
                "The ticket changed while you were editing it. Re-read it and try again."),

        InvalidCredentialsException ex =>
            (StatusCodes.Status401Unauthorized, "Authentication failed", ex.Message),

        AccountDisabledException ex =>
            (StatusCodes.Status403Forbidden, "Account disabled", ex.Message),

        InvalidOperationException ex when ex.Message.Contains("append-only") =>
            (StatusCodes.Status409Conflict, "Audit data is append-only", ex.Message),

        _ => (StatusCodes.Status500InternalServerError, "Internal server error",
            "An unexpected error occurred.")
    };
}
