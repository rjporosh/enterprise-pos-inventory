using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record GetAllStocksQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    bool? LowStock = null,
    bool? OutOfStock = null,
    string? SortBy = "productName",
    bool SortDescending = false) : IRequest<Result<PagedResult<StockListItemDto>>>;
