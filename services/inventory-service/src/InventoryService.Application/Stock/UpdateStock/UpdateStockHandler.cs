using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.Application.Stock;

public class UpdateStockHandler(
    ILogger<UpdateStockHandler> logger,
    IStockRepository repository) : IRequestHandler<UpdateStockCommand, Result<StockDto>>
{
    public async Task<Result<StockDto>> Handle(UpdateStockCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var stock = await repository.GetByIdAsync(request.Id, ct);

        if (stock is null)
        {
            return Result<StockDto>.Failure(new Error("STOCK_NOT_FOUND",
                $"Stock record with ID '{request.Id}' was not found."));
        }

        if (stock.IsDeleted)
        {
            return Result<StockDto>.Failure(new Error("STOCK_DELETED",
                $"Stock record with ID '{request.Id}' has been deleted."));
        }

        var existingStock = await repository.GetByProductAndWarehouseAsync(request.ProductId, request.WarehouseId, ct);
        if (existingStock is not null && existingStock.Id != stock.Id)
        {
            return Result<StockDto>.Failure(new Error("STOCK_ALREADY_EXISTS",
                $"A stock record already exists for product {request.ProductId} in warehouse {request.WarehouseId}."));
        }

        stock.ProductId = request.ProductId;
        stock.WarehouseId = request.WarehouseId;
        stock.UpdateSettings(request.ReorderLevel, request.MaxStockLevel);

        repository.Update(stock);
        await repository.SaveChangesAsync(ct);

        var updated = await repository.GetByIdAsync(request.Id, ct);
        if (updated is null)
        {
            return Result<StockDto>.Failure(new Error("STOCK_NOT_FOUND",
                "Stock record was updated but could not be retrieved."));
        }

        logger.LogInformation("Updated stock record {StockId}.", request.Id);

        return new StockDto(
            Id: updated.Id,
            ProductId: updated.ProductId,
            ProductName: updated.Product.Name,
            ProductSku: updated.Product.Sku,
            WarehouseId: updated.WarehouseId,
            WarehouseName: updated.Warehouse.Name,
            WarehouseCode: updated.Warehouse.Code ?? string.Empty,
            QuantityOnHand: updated.QuantityOnHand,
            QuantityReserved: updated.QuantityReserved,
            AvailableQuantity: updated.AvailableQuantity,
            ReorderLevel: updated.ReorderLevel,
            MaxStockLevel: updated.MaxStockLevel,
            LastRestockedAt: updated.LastRestockedAt,
            CreatedAt: updated.CreatedAt,
            UpdatedAt: updated.UpdatedAt);
    }
}
