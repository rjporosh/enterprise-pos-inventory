using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record StockOutCommand(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Notes) : IRequest<Result<StockMovementDto>>;
