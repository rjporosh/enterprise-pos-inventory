using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record StockTransferCommand(
    Guid ProductId,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    int Quantity,
    string? Notes) : IRequest<Result<StockMovementDto>>;
