using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.Application.Stock;

public class GetStockByIdHandler(
    ILogger<GetStockByIdHandler> logger,
    IStockRepository repository) : IRequestHandler<GetStockByIdQuery, Result<StockDto>>
{
    public async Task<Result<StockDto>> Handle(GetStockByIdQuery query, CancellationToken ct)
    {
        var stock = await repository.GetByIdAsync(query.Id, ct);

        if (stock is null)
        {
            logger.LogWarning("Stock record {StockId} not found.", query.Id);
            return Result<StockDto>.Failure(new Error("STOCK_NOT_FOUND", $"Stock record with ID '{query.Id}' was not found."));
        }

        var dto = new StockDto(
            Id: stock.Id,
            ProductId: stock.ProductId,
            ProductName: stock.Product.Name,
            ProductSku: stock.Product.Sku,
            WarehouseId: stock.WarehouseId,
            WarehouseName: stock.Warehouse.Name,
            WarehouseCode: stock.Warehouse.Code ?? string.Empty,
            QuantityOnHand: stock.QuantityOnHand,
            QuantityReserved: stock.QuantityReserved,
            AvailableQuantity: stock.AvailableQuantity,
            ReorderLevel: stock.ReorderLevel,
            MaxStockLevel: stock.MaxStockLevel,
            LastRestockedAt: stock.LastRestockedAt,
            CreatedAt: stock.CreatedAt,
            UpdatedAt: stock.UpdatedAt);

        return dto;
    }
}
