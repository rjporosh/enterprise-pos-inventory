using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using SharedKernel;

namespace SharedInfrastructure.Logging;

public static class SerilogConfiguration
{
    /// <param name="configuration">
    /// When provided and <c>Seq:Url</c> is set, logs are additionally shipped to Seq (the
    /// `enterprise-seq` container in docker-compose.yml, http://localhost:5341 by default) —
    /// this was previously never wired despite Seq running in every compose stack, so nothing
    /// ever reached it. Purely additive: omit the parameter or leave <c>Seq:Url</c> unset and
    /// behavior is identical to before (console + debug sinks only).
    /// </param>
    public static ILogger CreateLogger(string serviceName, string environment, IConfiguration? configuration = null)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console(new JsonFormatter())
            .WriteTo.Debug();

        var seqUrl = configuration?["Seq:Url"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            loggerConfig = loggerConfig.WriteTo.Seq(seqUrl);
        }

        return loggerConfig.CreateLogger();
    }
}
