using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SharedInfrastructure.Observability;

/// <summary>
/// Distributed tracing (OTLP -> Jaeger or any OTLP-compatible collector) and metrics (Prometheus scrape
/// endpoint) for a service. Tracing exports only if Observability:OtlpEndpoint is configured; metrics
/// (the /metrics endpoint) are always registered since they have no external dependency to reach. Neither
/// is required for the service's own core functionality to work — this is purely additive.
/// </summary>
public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, string serviceName, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["Observability:OtlpEndpoint"];

        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName: serviceName);

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }
}
