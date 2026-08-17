using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;
using InventoryService.Application.Products.Repositories;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.GetAllProducts;

public class GetAllProductsHandler(
    ILogger<GetAllProductsHandler> logger,
    IProductRepository repository) : IRequestHandler<GetAllProductsQuery, Result<PagedResult<ProductListItemDto>>>
{
    public async Task<Result<PagedResult<ProductListItemDto>>> Handle(GetAllProductsQuery query, CancellationToken ct)
    {
        var products = await repository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            query.CategoryId,
            query.BrandId,
            query.IsActive,
            query.SearchTerm,
            query.SortBy ?? "name",
            query.SortDescending,
            ct);

        var totalCount = await repository.GetTotalCountAsync(
            query.CategoryId,
            query.BrandId,
            query.IsActive,
            query.SearchTerm,
            ct);

        logger.LogInformation("Retrieved {Count} products (total: {Total})", products.Count, totalCount);

        var productDtos = products.Select(p => new ProductListItemDto(
            p.Id,
            p.Name,
            p.Sku,
            p.Barcode,
            p.Category.Name,
            p.Brand.Name,
            p.Unit.Symbol,
            p.SellingPrice,
            p.IsActive,
            p.ReorderLevel)).ToList();

        return new PagedResult<ProductListItemDto>(productDtos, totalCount, query.PageNumber, query.PageSize);
    }
}
