using System.Data.SqlClient;

namespace League.Middleware;

/// <summary>
/// The <see cref="TaskCanceledException"/> is commonly thrown when the connection to
/// the client is dropped, e.g. when a user closes or moves away from a web page
/// before a server-side task has completed.
/// This middleware catches and logs such exceptions.
/// </summary>
public class ClientAbortMiddleware : IMiddleware
{
    private readonly ILogger _logger;

    public ClientAbortMiddleware(ILogger<ClientAbortMiddleware> logger)
    {
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        // If the cancellation token has not been requested,
        // it means that the exception was caused by the client closing the connection.
        // Note: SqlException with message "Operation cancelled by user may throw, too, but rarely.
        catch (Exception ex) when (ex is TaskCanceledException or Microsoft.Data.SqlClient.SqlException && context.RequestAborted.IsCancellationRequested)
        {
            // Log the exception and stop the request queue.
            _logger.LogWarning(ex, "Request aborted by client. Processing stops.");
        }
    }
}
