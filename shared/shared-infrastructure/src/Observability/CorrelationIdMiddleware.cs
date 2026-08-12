using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace SharedInfrastructure.Observability;

/// <summary>
/// Reads X-Correlation-Id from the incoming request (or generates one), pushes it onto the Serilog
/// LogContext so every log line for this request carries it, tags the current Activity so it shows up
/// in traces, and echoes it back on the response. Applies uniformly to both services.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        Activity.Current?.SetTag("correlation_id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
