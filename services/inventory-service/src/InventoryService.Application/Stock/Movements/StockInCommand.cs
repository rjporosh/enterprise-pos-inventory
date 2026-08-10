using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record StockInCommand(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    decimal? UnitCost,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Notes) : IRequest<Result<StockMovementDto>>;
