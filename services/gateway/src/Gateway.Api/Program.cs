using System.Threading.RateLimiting;
using Gateway.Api.Middleware;
using HealthChecks.Uris;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

var serviceName = builder.Configuration["ServiceName"] ?? "gateway";
var environment = builder.Environment.EnvironmentName;

// ---------- Serilog ----------
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Information)
    .MinimumLevel.Override("Yarp", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", serviceName)
    .Enrich.WithProperty("Environment", environment)
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Debug();

var seqUrl = builder.Configuration["Seq:Url"];
if (!string.IsNullOrWhiteSpace(seqUrl))
{
    loggerConfig = loggerConfig.WriteTo.Seq(seqUrl);
}

Log.Logger = loggerConfig.CreateLogger();
builder.Host.UseSerilog(Log.Logger);

// ---------- YARP reverse proxy ----------
// Routes/clusters are entirely config-driven (see appsettings.json's ReverseProxy section) so
// adding, removing, or repointing a downstream route needs no code change or redeploy of this
// service's binary — only a config change.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ---------- CORS ----------
// This gateway is the single public origin the frontend apps should call once wired in (Phase 3
// exit criteria: "browser knows only public gateway origin") — each downstream service also still
// has its own CORS policy for now, since nothing has been repointed at the gateway yet.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

// ---------- Rate limiting ----------
// A general per-client-IP limit at the edge, ahead of any per-endpoint limits the individual
// services may add themselves (e.g. auth-service's stricter login/register limiter).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 200),
            Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60)),
            QueueLimit = 0
        });
    });
});

// ---------- Health checks ----------
// The gateway's own liveness, plus a fan-out check against every downstream service's /health —
// GET /health/services gives a single "is the whole platform up" view without hitting four
// different ports by hand.
var healthChecksBuilder = builder.Services.AddHealthChecks();
var downstreamServices = builder.Configuration.GetSection("Services").GetChildren();
foreach (var service in downstreamServices)
{
    var name = service.Key;
    var healthUrl = service["HealthUrl"];
    if (!string.IsNullOrWhiteSpace(healthUrl))
    {
        healthChecksBuilder.AddUrlGroup(new Uri(healthUrl), name: name, tags: ["downstream"]);
    }
}

// ---------- Observability ----------
var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName);
var otlpEndpoint = builder.Configuration["Observability:OtlpEndpoint"];
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(resourceBuilder)
            .AddSource(serviceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(otlpEndpoint));
        }
    })
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("AllowConfiguredOrigins");
app.UseRateLimiter();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => !check.Tags.Contains("downstream")
});
app.MapHealthChecks("/health/services", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("downstream"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            services = report.Entries.ToDictionary(
                e => e.Key,
                e => new { status = e.Value.Status.ToString(), description = e.Value.Description })
        };
        await context.Response.WriteAsJsonAsync(payload);
    }
});
app.MapPrometheusScrapingEndpoint("/metrics");

app.MapReverseProxy();

app.Run();

/// <summary>
/// Exposes the top-level-statements-generated Program class (implicitly internal) as public, so
/// WebApplicationFactory&lt;Program&gt; can reference this assembly's entry point from tests.
/// </summary>
public partial class Program { }
