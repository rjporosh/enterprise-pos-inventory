using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.Application.Stock;

public class StockTransferHandler(
    ILogger<StockTransferHandler> logger,
    IStockRepository repository) : IRequestHandler<StockTransferCommand, Result<StockMovementDto>>
{
    public async Task<Result<StockMovementDto>> Handle(StockTransferCommand command, CancellationToken ct)
    {
        var fromStock = await repository.GetByProductAndWarehouseAsync(command.ProductId, command.FromWarehouseId, ct);
        var toStock = await repository.GetByProductAndWarehouseAsync(command.ProductId, command.ToWarehouseId, ct);

        if (fromStock is null)
        {
            return Result<StockMovementDto>.Failure(new Error("STOCK_NOT_FOUND",
                $"No stock record found for product {command.ProductId} in source warehouse {command.FromWarehouseId}."));
        }

        if (fromStock.IsDeleted)
        {
            return Result<StockMovementDto>.Failure(new Error("STOCK_DELETED",
                $"Source stock record for product {command.ProductId} has been deleted."));
        }

        if (fromStock.AvailableQuantity < command.Quantity)
        {
            return Result<StockMovementDto>.Failure(new Error("INSUFFICIENT_STOCK",
                $"Insufficient stock in source warehouse. Available: {fromStock.AvailableQuantity}, Requested: {command.Quantity}."));
        }

        if (toStock is null)
        {
            toStock = new global::InventoryService.Domain.Stock.Stock(command.ProductId, command.ToWarehouseId);
            repository.Add(toStock);
        }
        else if (toStock.IsDeleted)
        {
            return Result<StockMovementDto>.Failure(new Error("STOCK_DELETED",
                $"Destination stock record for product {command.ProductId} in warehouse {command.ToWarehouseId} has been deleted."));
        }

        var fromNewBalance = fromStock.QuantityOnHand - command.Quantity;
        var toNewBalance = toStock.QuantityOnHand + command.Quantity;

        if (toStock.MaxStockLevel > 0 && toNewBalance > toStock.MaxStockLevel)
        {
            return Result<StockMovementDto>.Failure(new Error("MAX_STOCK_LEVEL_EXCEEDED",
                $"Transfer would exceed destination max stock level of {toStock.MaxStockLevel}."));
        }

        var transferRefId = Guid.NewGuid();

        var fromMovement = new global::InventoryService.Domain.Stock.StockMovement(
            stockId: fromStock.Id,
            productId: fromStock.ProductId,
            warehouseId: fromStock.WarehouseId,
            movementType: global::InventoryService.Domain.Stock.StockMovementType.TransferOut,
            quantity: -command.Quantity,
            balanceAfter: fromNewBalance,
            referenceType: "StockTransfer",
            referenceId: transferRefId,
            notes: $"Transferred to warehouse {command.ToWarehouseId}. {command.Notes}");

        var toMovement = new global::InventoryService.Domain.Stock.StockMovement(
            stockId: toStock.Id,
            productId: toStock.ProductId,
            warehouseId: toStock.WarehouseId,
            movementType: global::InventoryService.Domain.Stock.StockMovementType.TransferIn,
            quantity: command.Quantity,
            balanceAfter: toNewBalance,
            referenceType: "StockTransfer",
            referenceId: transferRefId,
            notes: $"Transferred from warehouse {command.FromWarehouseId}. {command.Notes}");

        fromStock.AddMovement(fromMovement);
        toStock.AddMovement(toMovement);

        if (toNewBalance > 0)
        {
            toStock.LastRestockedAt = DateTime.UtcNow;
        }

        repository.Update(fromStock);
        repository.Update(toStock);
        await repository.SaveChangesAsync(ct);

        var savedFrom = await repository.GetByIdAsync(fromStock.Id, ct);
        var savedFromMovement = savedFrom is not null ? savedFrom.Movements.LastOrDefault() : null;

        if (savedFromMovement is null)
        {
            return Result<StockMovementDto>.Failure(new Error("MOVEMENT_NOT_SAVED",
                "Stock transfer could not be saved."));
        }

        logger.LogInformation("Stock Transfer: {Quantity} units of product {ProductId} from warehouse {FromWarehouse} to warehouse {ToWarehouse}. From balance: {FromBalance}, To balance: {ToBalance}",
            command.Quantity, command.ProductId, command.FromWarehouseId, command.ToWarehouseId, fromNewBalance, toNewBalance);

        return ToDto(savedFrom!, savedFromMovement);
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
