using Microsoft.Extensions.Options;

namespace SharedInfrastructure.Logging;

public record struct LoggingOptions(string ServiceName, string Environment, string OutputTemplate);
