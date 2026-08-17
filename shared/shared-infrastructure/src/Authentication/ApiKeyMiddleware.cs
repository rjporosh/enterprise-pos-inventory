using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SharedInfrastructure.Authentication;

/// <summary>
/// Lightweight API-key authentication middleware.
///
/// How it works:
///   1. Reads the configured API key from ApiAuth:ApiKey in appsettings.
///   2. Expects clients to send the key in the X-Api-Key request header.
///   3. Returns 401 Unauthorized if the header is missing or wrong.
///   4. Returns 503 Service Unavailable (with a log warning) if no key is configured,
///      to avoid silently running an open API in production.
///
/// Bypass paths:
///   The following paths are always allowed without a key so infrastructure probes keep working:
///   /health, /health/live, /health/ready, /metrics, /openapi, /scalar
///
/// Disabling:
///   Set ApiAuth:Enabled = false (or omit it) in appsettings to skip the middleware entirely.
///   This is the default for development so the service can be run immediately without any config.
///
/// This is a stepping-stone toward full JWT/OAuth2 (ADR planned — see ai-handover.md). It gives
/// every endpoint a meaningful auth boundary without requiring an identity server to be running.
/// </summary>
public class ApiKeyMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    ILogger<ApiKeyMiddleware> logger)
{
    public const string ApiKeyHeader = "X-Api-Key";

    private static readonly string[] BypassPaths =
    [
        "/health", "/metrics", "/openapi", "/scalar", "/favicon"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var enabled = configuration.GetValue<bool?>("ApiAuth:Enabled") ?? false;

        if (!enabled)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (BypassPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var configuredKey = configuration["ApiAuth:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            logger.LogWarning("ApiAuth is enabled but ApiAuth:ApiKey is not configured — blocking all requests");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("API key authentication is enabled but no key is configured.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) ||
            !string.Equals(configuredKey, providedKey.ToString(), StringComparison.Ordinal))
        {
            logger.LogWarning("Rejected request to {Path} — missing or invalid {Header}", path, ApiKeyHeader);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: invalid or missing API key.");
            return;
        }

        await next(context);
    }
}
