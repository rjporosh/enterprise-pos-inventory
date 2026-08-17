using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace SharedInfrastructure.RateLimiting;

/// <summary>
/// Configures sliding-window rate limiting for all API endpoints.
///
/// Policy names:
///   "api"     — general REST API calls (default)
///   "health"  — health-check endpoints (very generous; must not block probes)
///   "write"   — mutating endpoints (POST/PUT/DELETE); tighter window
///
/// All limits are driven by appsettings so they can be tuned per environment without
/// a rebuild. Defaults are intentionally permissive for development:
///
/// RateLimiting:
///   Api:    { PermitLimit: 100, WindowSeconds: 60 }
///   Write:  { PermitLimit: 30,  WindowSeconds: 60 }
///   Health: { PermitLimit: 300, WindowSeconds: 60 }
///
/// The middleware is a no-op when RateLimiting:Enabled is false (or absent).
/// </summary>
public static class RateLimitingExtensions
{
    public const string ApiPolicy = "api";
    public const string WritePolicy = "write";
    public const string HealthPolicy = "health";

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool?>("RateLimiting:Enabled") ?? false;
        if (!enabled)
        {
            return services;
        }

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddSlidingWindowLimiter(ApiPolicy, opt =>
            {
                opt.PermitLimit = configuration.GetValue<int?>("RateLimiting:Api:PermitLimit") ?? 100;
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int?>("RateLimiting:Api:WindowSeconds") ?? 60);
                opt.SegmentsPerWindow = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            options.AddSlidingWindowLimiter(WritePolicy, opt =>
            {
                opt.PermitLimit = configuration.GetValue<int?>("RateLimiting:Write:PermitLimit") ?? 30;
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int?>("RateLimiting:Write:WindowSeconds") ?? 60);
                opt.SegmentsPerWindow = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });

            options.AddSlidingWindowLimiter(HealthPolicy, opt =>
            {
                opt.PermitLimit = configuration.GetValue<int?>("RateLimiting:Health:PermitLimit") ?? 300;
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int?>("RateLimiting:Health:WindowSeconds") ?? 60);
                opt.SegmentsPerWindow = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 50;
            });

            // Global fallback: if an endpoint has no explicit policy it still gets the API limit
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: "global",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = configuration.GetValue<int?>("RateLimiting:Global:PermitLimit") ?? 500,
                        Window = TimeSpan.FromSeconds(configuration.GetValue<int?>("RateLimiting:Global:WindowSeconds") ?? 60),
                        SegmentsPerWindow = 6,
                    }));
        });

        return services;
    }

    /// <summary>
    /// Conditionally activates rate limiting middleware. Must be called after UseRouting()
    /// and before UseAuthorization() / MapControllers().
    /// </summary>
    public static IApplicationBuilder UseApiRateLimiting(this IApplicationBuilder app, IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool?>("RateLimiting:Enabled") ?? false;
        if (enabled)
        {
            app.UseRateLimiter();
        }

        return app;
    }
}
