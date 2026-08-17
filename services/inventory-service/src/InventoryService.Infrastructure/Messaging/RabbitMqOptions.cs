namespace InventoryService.Infrastructure.Messaging;

/// <summary>
/// Bound from the "RabbitMQ" configuration section. When Host is null/empty, the SaleEventsConsumer
/// hosted service stays idle — RabbitMQ is opt-in, never mandatory for Inventory's own core functionality.
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
    public string Queue { get; set; } = "inventory.pos-sale-events";
    public string DeadLetterQueue { get; set; } = "inventory.pos-sale-events.dlq";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
