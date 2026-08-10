using InventoryService.Domain.Common;
using InventoryService.Domain.Catalog;
using InventoryService.Domain.Suppliers;

namespace InventoryService.Domain.Products;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public Guid UnitId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? TaxPercent { get; set; }
    public int ReorderLevel { get; set; } = 0;
    public int MaxStockLevel { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool TrackInventory { get; set; } = true;

    public Category Category { get; set; } = null!;
    public Brand Brand { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
    public Supplier? Supplier { get; set; }

    public Product() { }

    public Product(string name, string sku, Guid categoryId, Guid brandId, Guid unitId, decimal costPrice, decimal sellingPrice)
    {
        Name = SharedKernel.Guard.NotNullOrEmpty(name, nameof(name));
        Sku = SharedKernel.Guard.NotNullOrEmpty(sku, nameof(sku));
        CategoryId = categoryId;
        BrandId = brandId;
        UnitId = unitId;
        CostPrice = SharedKernel.Guard.NotNegative(costPrice, nameof(costPrice));
        SellingPrice = SharedKernel.Guard.NotNegative(sellingPrice, nameof(sellingPrice));
    }

    public void UpdatePrice(decimal costPrice, decimal sellingPrice)
    {
        CostPrice = SharedKernel.Guard.NotNegative(costPrice, nameof(costPrice));
        SellingPrice = SharedKernel.Guard.NotNegative(sellingPrice, nameof(sellingPrice));
    }

    public void SetBarcode(string? barcode)
    {
        Barcode = barcode;
    }
}
