using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;
using InventoryService.Application.Products.Repositories;

namespace InventoryService.Application.Products.UpdateProduct;

public class UpdateProductHandler(
    ILogger<UpdateProductHandler> logger,
    IProductRepository repository) : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var product = await repository.GetByIdAsync(request.Id, ct);

        if (product is null)
        {
            return Result.Failure(new Error("PRODUCT_NOT_FOUND", $"Product with ID '{request.Id}' was not found."));
        }

        if (product.IsDeleted)
        {
            return Result.Failure(new Error("PRODUCT_DELETED", $"Product with ID '{request.Id}' has been deleted."));
        }

        if (await repository.SkuExistsAsync(request.Sku, excludeId: request.Id))
        {
            return Result.Failure(new Error("PRODUCT_SKU_EXISTS", $"Product with SKU '{request.Sku}' already exists."));
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode) && await repository.BarcodeExistsAsync(request.Barcode, excludeId: request.Id))
        {
            return Result.Failure(new Error("PRODUCT_BARCODE_EXISTS", $"Product with barcode '{request.Barcode}' already exists."));
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.Barcode = request.Barcode;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.UnitId = request.UnitId;
        product.SupplierId = request.SupplierId;
        product.CostPrice = request.CostPrice;
        product.SellingPrice = request.SellingPrice;
        product.DiscountPercent = request.DiscountPercent;
        product.TaxPercent = request.TaxPercent;
        product.ReorderLevel = request.ReorderLevel;
        product.MaxStockLevel = request.MaxStockLevel;
        product.IsActive = request.IsActive;
        product.TrackInventory = request.TrackInventory;

        repository.Update(product);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Updated product {ProductId}", product.Id);

        return Result.Success();
    }
}
