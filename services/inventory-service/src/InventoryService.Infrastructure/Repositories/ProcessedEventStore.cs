using InventoryService.Application.Integration;
using InventoryService.Domain.Integration;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class ProcessedEventStore(InventoryDbContext context) : IProcessedEventStore
{
    public async Task<bool> IsProcessedAsync(Guid eventId, CancellationToken ct = default)
        => await context.ProcessedIntegrationEvents.AnyAsync(e => e.EventId == eventId, ct);

    public async Task MarkProcessedAsync(Guid eventId, string eventType, CancellationToken ct = default)
    {
        context.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent(eventId, eventType));
        await context.SaveChangesAsync(ct);
    }
}
