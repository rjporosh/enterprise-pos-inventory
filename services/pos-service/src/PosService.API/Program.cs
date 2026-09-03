global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Scalar.AspNetCore;
global using SharedInfrastructure.Logging;
global using SharedInfrastructure;
global using SharedInfrastructure.Observability;
global using SharedInfrastructure.Persistence;
global using SharedWeb;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Http;
global using System.Text.Json;
global using SharedKernel;
global using FluentValidation;
global using Serilog;
global using PosService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

var serviceName = builder.Configuration["ServiceName"] ?? "pos-service";
var environment = builder.Environment.EnvironmentName;
var logger = SerilogConfiguration.CreateLogger(serviceName, environment, builder.Configuration);
builder.Host.UseSerilog(logger);

builder.Services.AddObservability(serviceName, builder.Configuration);

var servicesConfig = builder.Configuration.GetSection("Services");
builder.Services.AddHealthChecks()
    .AddCheck("self", () =>
    {
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("POS Service is running");
    });

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSharedInfrastructure(typeof(PosService.Application.Sales.CreateSale.CreateSaleCommand).Assembly);
builder.Services.AddDatabaseProvider(builder.Configuration);
builder.Services.AddPlatformExceptionHandling();

builder.Services.AddScoped(sp =>
{
    var dbContextFactory = sp.GetRequiredService<IDbContextFactory>();
    var connectionString = builder.Configuration["Database:ConnectionString"]
        ?? throw new InvalidOperationException("Database:ConnectionString is not configured.");
    var options = dbContextFactory.CreateOptions<PosService.Infrastructure.Persistence.PosDbContext>(connectionString);
    return new PosService.Infrastructure.Persistence.PosDbContext(options);
});

builder.Services.AddScoped<PosService.Application.Stores.IStoreRepository, PosService.Infrastructure.Repositories.StoreRepository>();
builder.Services.AddScoped<PosService.Application.Cashiers.ICashierRepository, PosService.Infrastructure.Repositories.CashierRepository>();
builder.Services.AddScoped<PosService.Application.Registers.ICashRegisterRepository, PosService.Infrastructure.Repositories.CashRegisterRepository>();
builder.Services.AddScoped<PosService.Application.Registers.ICashSessionRepository, PosService.Infrastructure.Repositories.CashSessionRepository>();
builder.Services.AddScoped<PosService.Application.Customers.ICustomerRepository, PosService.Infrastructure.Repositories.CustomerRepository>();
builder.Services.AddScoped<PosService.Application.Sales.Repositories.ISaleRepository, PosService.Infrastructure.Repositories.SaleRepository>();
builder.Services.AddScoped<PosService.Application.Reporting.IDailySalesReportRepository, PosService.Infrastructure.Repositories.DailySalesReportRepository>();
builder.Services.AddScoped<PosService.Application.Reporting.IDailySalesReportGenerator, PosService.Application.Reporting.DailySalesReportGenerator>();
builder.Services.AddHostedService<PosService.Infrastructure.Reporting.DailySalesReportJob>();

// No message broker is registered by default so POS's own checkout flow never hard-depends on RabbitMQ
// (PRIMARY GOAL: RabbitMQ must not become mandatory for either service's independent core functionality).
// AddPosMessaging(builder.Configuration) below only overrides this with a real publisher when configured.
builder.Services.AddScoped<PosService.Application.Sales.Events.ISaleEventPublisher, PosService.Application.Sales.Events.NullSaleEventPublisher>();
builder.Services.AddPosMessaging(builder.Configuration);

builder.Services.AddResponseCaching();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("POS Service API")
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseMiddleware<SharedInfrastructure.Observability.CorrelationIdMiddleware>();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseCors("Default");

app.UseResponseCaching();

app.UseRouting();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint();
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.MapControllers();

app.MapGet("/api/v1/system/release", (IConfiguration configuration, IWebHostEnvironment env) =>
{
    var assembly = typeof(Program).Assembly;
    var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
    var commit = "local-development";
    var buildTime = File.GetLastWriteTime(assembly.Location).ToString("yyyyMMdd.HHmmss");

    var response = new
    {
        service = "pos-service",
        version = version,
        build = buildTime,
        commit = commit,
        environment = env.EnvironmentName,
        apiVersion = "v1",
        databaseMigration = "pending",
        features = new List<string>(),
        releaseNotes = new List<string> { "Initial backend foundation" },
        knownIssues = new List<string>()
    };

    return Results.Json(response);
})
.WithName("GetReleaseInfo")
.WithTags("System")
.Produces(200);

app.Run();

/// <summary>
/// Exposes the top-level-statements-generated Program class (implicitly internal)
/// as public, so <c>WebApplicationFactory&lt;Program&gt;</c> can reference this
/// assembly's entry point from the integration test project.
/// </summary>
public partial class Program { }
