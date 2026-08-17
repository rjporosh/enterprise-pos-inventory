namespace InventoryService.Application.Products.Dtos;

public record UpdateProductRequest(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    string? Barcode,
    Guid CategoryId,
    Guid BrandId,
    Guid UnitId,
    Guid? SupplierId,
    decimal CostPrice,
    decimal SellingPrice,
    decimal? DiscountPercent,
    decimal? TaxPercent,
    int ReorderLevel,
    int MaxStockLevel,
    bool IsActive,
    bool TrackInventory);
