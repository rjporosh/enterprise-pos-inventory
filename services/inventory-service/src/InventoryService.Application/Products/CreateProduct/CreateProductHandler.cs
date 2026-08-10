using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;
using InventoryService.Domain.Products;
using InventoryService.Application.Products.Repositories;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.CreateProduct;

public class CreateProductHandler(
    ILogger<CreateProductHandler> logger,
    IProductRepository repository) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var request = command.Request;

        if (await repository.SkuExistsAsync(request.Sku))
        {
            return Result<Guid>.Failure(new Error("PRODUCT_SKU_EXISTS", $"Product with SKU '{request.Sku}' already exists."));
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode) && await repository.BarcodeExistsAsync(request.Barcode))
        {
            return Result<Guid>.Failure(new Error("PRODUCT_BARCODE_EXISTS", $"Product with barcode '{request.Barcode}' already exists."));
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Sku = request.Sku,
            Barcode = request.Barcode,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            UnitId = request.UnitId,
            SupplierId = request.SupplierId,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            DiscountPercent = request.DiscountPercent,
            TaxPercent = request.TaxPercent,
            ReorderLevel = request.ReorderLevel,
            MaxStockLevel = request.MaxStockLevel,
            TrackInventory = request.TrackInventory
        };

        repository.Add(product);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Created product {ProductId} with SKU {Sku}", product.Id, product.Sku);

        return product.Id;
    }
}
