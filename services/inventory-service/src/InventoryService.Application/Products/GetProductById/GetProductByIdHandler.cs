using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;
using InventoryService.Application.Products.Repositories;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.GetProductById;

public class GetProductByIdHandler(
    ILogger<GetProductByIdHandler> logger,
    IProductRepository repository) : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(query.Id, ct);

        if (product is null)
        {
            return Result<ProductDto>.Failure(new Error("PRODUCT_NOT_FOUND", $"Product with ID '{query.Id}' was not found."));
        }

        if (product.IsDeleted)
        {
            return Result<ProductDto>.Failure(new Error("PRODUCT_DELETED", $"Product with ID '{query.Id}' has been deleted."));
        }

        logger.LogInformation("Retrieved product {ProductId}", product.Id);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.Barcode,
            product.CategoryId,
            product.BrandId,
            product.UnitId,
            product.SupplierId,
            product.CostPrice,
            product.SellingPrice,
            product.DiscountPercent,
            product.TaxPercent,
            product.ReorderLevel,
            product.MaxStockLevel,
            product.IsActive,
            product.TrackInventory,
            product.CreatedAt);
    }
}
