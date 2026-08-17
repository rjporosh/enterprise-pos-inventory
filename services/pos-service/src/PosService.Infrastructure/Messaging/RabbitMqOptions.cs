namespace PosService.Infrastructure.Messaging;

/// <summary>
/// Bound from the "RabbitMQ" configuration section. When Host is null/empty, messaging stays disabled
/// and POS keeps using the no-op ISaleEventPublisher — RabbitMQ is opt-in, never mandatory.
/// </summary>
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string? Host { get; set; }
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "pos.events";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
