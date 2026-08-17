using PosService.Domain.Sales;

namespace PosService.Application.Sales.Events;

/// <summary>
/// Publishes POS domain events for optional cross-service integration (e.g. notifying Inventory to
/// deduct stock when a sale completes). POS's own checkout flow must succeed regardless of whether a
/// real publisher is wired up — RabbitMQ is never a mandatory dependency for POS's core functionality
/// (ADR-001 / PRIMARY GOAL). The default registration is a no-op; Infrastructure provides a RabbitMQ-backed
/// implementation that is only registered when messaging is configured.
/// </summary>
public interface ISaleEventPublisher
{
    Task PublishSaleCompletedAsync(Sale sale, CancellationToken ct = default);
    Task PublishSaleVoidedAsync(Sale sale, CancellationToken ct = default);
}
