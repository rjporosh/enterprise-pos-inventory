global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Scalar.AspNetCore;
global using SharedInfrastructure.Logging;
global using SharedInfrastructure;
global using SharedInfrastructure.Persistence;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Http;
global using System.Text.Json;
global using SharedKernel;
global using FluentValidation;
global using Serilog;

var builder = WebApplication.CreateBuilder(args);

var serviceName = builder.Configuration["ServiceName"] ?? "inventory-service";
var environment = builder.Environment.EnvironmentName;
var logger = SerilogConfiguration.CreateLogger(serviceName, environment);
builder.Host.UseSerilog(logger);

var servicesConfig = builder.Configuration.GetSection("Services");
builder.Services.AddHealthChecks()
    .AddCheck("self", () =>
    {
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Inventory Service is running");
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

builder.Services.AddSharedInfrastructure();
builder.Services.AddDatabaseProvider(builder.Configuration);

builder.Services.AddScoped<InventoryService.Application.Products.Repositories.IProductRepository, InventoryService.Infrastructure.Repositories.ProductRepository>();

builder.Services.AddResponseCaching();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"])
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
        options.WithTitle("Inventory Service API")
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseSerilogRequestLogging();

app.UseMiddleware<InventoryService.API.Middleware.GlobalExceptionHandler>();

app.UseCors("Default");

app.UseResponseCaching();

app.UseRouting();

app.UseAuthorization();

app.MapHealthChecks("/health");
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
        service = "inventory-service",
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
