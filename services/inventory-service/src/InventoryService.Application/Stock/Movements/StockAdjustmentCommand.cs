using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record StockAdjustmentCommand(
    Guid ProductId,
    Guid WarehouseId,
    int QuantityChange,
    string? Notes) : IRequest<Result<StockMovementDto>>;
