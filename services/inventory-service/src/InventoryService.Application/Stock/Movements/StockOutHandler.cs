using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.Application.Stock;

public class StockOutHandler(
    ILogger<StockOutHandler> logger,
    IStockRepository repository) : IRequestHandler<StockOutCommand, Result<StockMovementDto>>
{
    public async Task<Result<StockMovementDto>> Handle(StockOutCommand command, CancellationToken ct)
    {
        var stock = await repository.GetByProductAndWarehouseAsync(command.ProductId, command.WarehouseId, ct);

        if (stock is null)
        {
            return Result<StockMovementDto>.Failure(new Error("STOCK_NOT_FOUND",
                $"No stock record found for product {command.ProductId} in warehouse {command.WarehouseId}."));
        }

        if (stock.IsDeleted)
        {
            return Result<StockMovementDto>.Failure(new Error("STOCK_DELETED",
                $"Stock record for product {command.ProductId} has been deleted."));
        }

        if (stock.AvailableQuantity < command.Quantity)
        {
            return Result<StockMovementDto>.Failure(new Error("INSUFFICIENT_STOCK",
                $"Insufficient stock. Available: {stock.AvailableQuantity}, Requested: {command.Quantity}."));
        }

        var newBalance = stock.QuantityOnHand - command.Quantity;
        var movement = new global::InventoryService.Domain.Stock.StockMovement(
            stockId: stock.Id,
            productId: stock.ProductId,
            warehouseId: stock.WarehouseId,
            movementType: global::InventoryService.Domain.Stock.StockMovementType.StockOut,
            quantity: -command.Quantity,
            balanceAfter: newBalance,
            referenceType: command.ReferenceType,
            referenceId: command.ReferenceId,
            notes: command.Notes);

        stock.AddMovement(movement);
        repository.Update(stock);
        await repository.SaveChangesAsync(ct);

        var saved = await repository.GetByIdAsync(stock.Id, ct);
        var savedMovement = saved is not null ? saved.Movements.LastOrDefault() : null;

        if (savedMovement is null)
        {
            return Result<StockMovementDto>.Failure(new Error("MOVEMENT_NOT_SAVED",
                "Stock Out movement could not be saved."));
        }

        logger.LogInformation("Stock Out: {Quantity} units of product {ProductId} from warehouse {WarehouseId}. New balance: {Balance}",
            command.Quantity, command.ProductId, command.WarehouseId, newBalance);

        return ToDto(saved, savedMovement);
    }

    private static StockMovementDto ToDto(global::InventoryService.Domain.Stock.Stock stock, global::InventoryService.Domain.Stock.StockMovement movement) => new(
        Id: movement.Id,
        StockId: movement.StockId,
        ProductId: movement.ProductId,
        ProductName: stock.Product.Name,
        WarehouseId: movement.WarehouseId,
        WarehouseName: stock.Warehouse.Name,
        MovementType: movement.MovementType,
        Quantity: movement.Quantity,
        BalanceAfter: movement.BalanceAfter,
        UnitCost: movement.UnitCost,
        ReferenceType: movement.ReferenceType,
        ReferenceId: movement.ReferenceId,
        Notes: movement.Notes,
        CreatedAt: movement.CreatedAt);
}
