using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using SharedKernel;

namespace SharedInfrastructure.Logging;

public static class SerilogConfiguration
{
    public static ILogger CreateLogger(string serviceName, string environment)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console(new JsonFormatter())
            .WriteTo.Debug()
            .CreateLogger();
    }
}
