namespace InventoryService.Application.Products.Dtos;

public record ProductDto(
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
    bool TrackInventory,
    DateTime CreatedAt);

public record ProductListItemDto(
    Guid Id,
    string Name,
    string Sku,
    string? Barcode,
    string CategoryName,
    string BrandName,
    string UnitSymbol,
    decimal SellingPrice,
    bool IsActive,
    int ReorderLevel);
