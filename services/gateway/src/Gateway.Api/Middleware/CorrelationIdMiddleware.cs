using System.Diagnostics;
using Serilog.Context;

namespace Gateway.Api.Middleware;

/// <summary>
/// Reads X-Correlation-Id from the incoming request (or generates one), pushes it onto the
/// Serilog LogContext, tags the current Activity, and echoes it back on the response — same
/// contract as SharedInfrastructure.Observability.CorrelationIdMiddleware used by the four
/// downstream services, duplicated here rather than referenced (see Gateway.Api.csproj for why).
/// Because this runs before YARP's proxy step, the header is already present on
/// HttpContext.Request and flows through to whichever downstream service handles the request.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Request.Headers[HeaderName] = correlationId;
        context.Items["CorrelationId"] = correlationId;

        // Not also set on context.Response.Headers here: every downstream service already
        // echoes this same header back on its own response (see each service's
        // CorrelationIdMiddleware), and YARP copies that response through unchanged — setting
        // it here too would just duplicate an identical header value on every proxied response.
        Activity.Current?.SetTag("correlation_id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
