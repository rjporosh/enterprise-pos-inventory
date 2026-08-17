namespace InventoryService.Application.Integration;

public interface IProcessedEventStore
{
    Task<bool> IsProcessedAsync(Guid eventId, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid eventId, string eventType, CancellationToken ct = default);
}
