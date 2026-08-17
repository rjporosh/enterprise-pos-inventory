using Microsoft.Extensions.Logging;
using PosService.Domain.Sales;

namespace PosService.Application.Sales.Events;

/// <summary>Default no-op publisher used when no message broker is configured. Keeps POS fully
/// functional standalone; swapped out for a real broker-backed publisher when integration is enabled.</summary>
public class NullSaleEventPublisher(ILogger<NullSaleEventPublisher> logger) : ISaleEventPublisher
{
    public Task PublishSaleCompletedAsync(Sale sale, CancellationToken ct = default)
    {
        logger.LogDebug("No integration publisher configured; skipping SaleCompleted event for sale {SaleId}", sale.Id);
        return Task.CompletedTask;
    }

    public Task PublishSaleVoidedAsync(Sale sale, CancellationToken ct = default)
    {
        logger.LogDebug("No integration publisher configured; skipping SaleVoided event for sale {SaleId}", sale.Id);
        return Task.CompletedTask;
    }
}
