using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.Application.Stock;

public class GetAllStocksHandler(
    ILogger<GetAllStocksHandler> logger,
    IStockRepository repository) : IRequestHandler<GetAllStocksQuery, Result<PagedResult<StockListItemDto>>>
{
    public async Task<Result<PagedResult<StockListItemDto>>> Handle(GetAllStocksQuery query, CancellationToken ct)
    {
        IReadOnlyList<global::InventoryService.Domain.Stock.Stock> stocks;

        if (query.OutOfStock == true)
        {
            stocks = await repository.GetOutOfStockAsync(ct);
        }
        else if (query.LowStock == true)
        {
            stocks = await repository.GetLowStockAsync(ct);
        }
        else if (query.ProductId.HasValue && query.WarehouseId.HasValue)
        {
            var stock = await repository.GetByProductAndWarehouseAsync(query.ProductId.Value, query.WarehouseId.Value, ct);
            stocks = stock is not null ? [stock] : [];
        }
        else if (query.ProductId.HasValue)
        {
            stocks = await repository.GetByProductIdAsync(query.ProductId.Value, ct);
        }
        else if (query.WarehouseId.HasValue)
        {
            stocks = await repository.GetByWarehouseIdAsync(query.WarehouseId.Value, ct);
        }
        else
        {
            stocks = await repository.GetAllAsync(ct);
        }

        var items = stocks
            .Where(s => !s.IsDeleted)
            .Select(s => new StockListItemDto(
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
                IsLowStock: s.QuantityOnHand <= s.ReorderLevel && s.QuantityOnHand > 0,
                LastRestockedAt: s.LastRestockedAt))
            .ToList();

        var totalCount = items.Count;

        var sortByLower = query.SortBy?.ToLower() ?? "productname";
        var sorted = sortByLower switch
        {
            "productname" => query.SortDescending ? items.OrderByDescending(i => i.ProductName).ToList() : items.OrderBy(i => i.ProductName).ToList(),
            "warehousename" => query.SortDescending ? items.OrderByDescending(i => i.WarehouseName).ToList() : items.OrderBy(i => i.WarehouseName).ToList(),
            "quantityonhand" => query.SortDescending ? items.OrderByDescending(i => i.QuantityOnHand).ToList() : items.OrderBy(i => i.QuantityOnHand).ToList(),
            "availablequantity" => query.SortDescending ? items.OrderByDescending(i => i.AvailableQuantity).ToList() : items.OrderBy(i => i.AvailableQuantity).ToList(),
            "lastrestockedat" => query.SortDescending
                ? items.OrderByDescending(i => i.LastRestockedAt).ThenBy(i => i.ProductName).ToList()
                : items.OrderBy(i => i.LastRestockedAt).ThenBy(i => i.ProductName).ToList(),
            _ => query.SortDescending ? items.OrderByDescending(i => i.ProductName).ToList() : items.OrderBy(i => i.ProductName).ToList()
        };

        var pagedItems = sorted
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var result = new PagedResult<StockListItemDto>(
            Items: pagedItems,
            TotalCount: totalCount,
            PageNumber: query.PageNumber,
            PageSize: query.PageSize);

        return result;
    }
}
