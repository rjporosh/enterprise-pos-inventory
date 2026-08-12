namespace InventoryService.Domain.Integration;

/// <summary>
/// Records the ID of every inbound integration event (from POS) that has been successfully processed,
/// so at-least-once RabbitMQ delivery never double-applies a stock movement. Not a BaseEntity: it has no
/// tenant/soft-delete/audit semantics of its own — it is purely a dedupe ledger.
/// </summary>
public class ProcessedIntegrationEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; }

    public ProcessedIntegrationEvent() { }

    public ProcessedIntegrationEvent(Guid eventId, string eventType)
    {
        EventId = eventId;
        EventType = eventType;
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
