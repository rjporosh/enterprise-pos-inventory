namespace SharedKernel.IntegrationEvents;

/// <summary>
/// Line-item detail carried on Sale integration events. Only the fields Inventory needs to adjust
/// stock are included — POS never exposes its Sale aggregate or database to Inventory (ADR-001).
/// </summary>
public record SaleLineItem(Guid ProductId, string Sku, int Quantity);

/// <summary>
/// Published by POS when a sale is completed. Consumed by Inventory (if the integration is enabled) to
/// deduct stock. Optional: POS's checkout succeeds and remains the source of truth regardless of whether
/// anything consumes this event.
/// </summary>
public record SaleCompletedIntegrationEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTime OccurredAtUtc,
    Guid SaleId,
    string SaleNumber,
    Guid StoreId,
    IReadOnlyList<SaleLineItem> Items)
{
    public const string RoutingKey = "sale.completed";
}

/// <summary>
/// Published by POS when a previously-completed sale is voided, so Inventory can reverse the stock
/// deduction it applied for the original SaleCompleted event.
/// </summary>
public record SaleVoidedIntegrationEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTime OccurredAtUtc,
    Guid SaleId,
    string SaleNumber,
    Guid StoreId,
    IReadOnlyList<SaleLineItem> Items)
{
    public const string RoutingKey = "sale.voided";
}
