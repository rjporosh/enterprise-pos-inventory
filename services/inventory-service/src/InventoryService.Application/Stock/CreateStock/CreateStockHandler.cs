using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.Application.Stock;

public class CreateStockHandler(
    ILogger<CreateStockHandler> logger,
    IStockRepository repository) : IRequestHandler<CreateStockCommand, Result<StockDto>>
{
    public async Task<Result<StockDto>> Handle(CreateStockCommand command, CancellationToken ct)
    {
        var request = command.Request;

        if (await repository.ExistsForProductWarehouseAsync(request.ProductId, request.WarehouseId, ct))
        {
            return Result<StockDto>.Failure(new Error("STOCK_ALREADY_EXISTS",
                $"Stock record already exists for product {request.ProductId} in warehouse {request.WarehouseId}."));
        }

        var stock = new global::InventoryService.Domain.Stock.Stock(
            productId: request.ProductId,
            warehouseId: request.WarehouseId,
            reorderLevel: request.ReorderLevel,
            maxStockLevel: request.MaxStockLevel);

        if (request.InitialQuantity > 0)
        {
            var movement = new global::InventoryService.Domain.Stock.StockMovement(
                stockId: stock.Id,
                productId: stock.ProductId,
                warehouseId: stock.WarehouseId,
                movementType: global::InventoryService.Domain.Stock.StockMovementType.StockIn,
                quantity: request.InitialQuantity,
                balanceAfter: request.InitialQuantity,
                unitCost: request.UnitCost,
                notes: "Initial stock");

            stock.AddMovement(movement);
        }

        stock.LastRestockedAt = request.InitialQuantity > 0 ? DateTime.UtcNow : null;

        repository.Add(stock);
        await repository.SaveChangesAsync(ct);

        var created = await repository.GetByIdAsync(stock.Id, ct);
        if (created is null)
        {
            return Result<StockDto>.Failure(new Error("STOCK_NOT_FOUND",
                "Stock was created but could not be retrieved."));
        }

        logger.LogInformation("Created stock record {StockId} for product {ProductId} in warehouse {WarehouseId}",
            stock.Id, stock.ProductId, stock.WarehouseId);

        return ToDto(created);
    }

    private static StockDto ToDto(global::InventoryService.Domain.Stock.Stock s) => new(
        Id: s.Id,
        ProductId: s.ProductId,
        ProductName: s.Product.Name,
        ProductSku: s.Product.Sku,
        WarehouseId: s.WarehouseId,
        WarehouseName: s.Warehouse.Name,
        WarehouseCode: s.Warehouse.Code ?? string.Empty,
        QuantityOnHand: s.QuantityOnHand,
        QuantityReserved: s.QuantityReserved,
        AvailableQuantity: s.AvailableQuantity,
        ReorderLevel: s.ReorderLevel,
        MaxStockLevel: s.MaxStockLevel,
        LastRestockedAt: s.LastRestockedAt,
        CreatedAt: s.CreatedAt,
        UpdatedAt: s.UpdatedAt);
}
