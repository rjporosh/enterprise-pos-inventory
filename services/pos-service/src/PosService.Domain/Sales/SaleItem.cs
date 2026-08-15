using PosService.Domain.Common;
using SharedKernel;
using BaseEntity = PosService.Domain.Common.BaseEntity;

namespace PosService.Domain.Sales;

/// <summary>
/// A single line item on a sale. ProductId references a Product in the Inventory service by ID only
/// (no FK, no cross-service EF entity — per ADR-001). Name/Sku are denormalized snapshots taken at the
/// time of sale so a receipt remains accurate even if the product is later renamed in Inventory.
/// </summary>
public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public Sale Sale { get; set; } = null!;

    public SaleItem() { }

    public SaleItem(Guid saleId, Guid productId, string productName, string sku, decimal unitPrice, int quantity)
    {
        SaleId = saleId;
        ProductId = productId;
        ProductName = Guard.NotNullOrEmpty(productName, nameof(productName));
        Sku = Guard.NotNullOrEmpty(sku, nameof(sku));
        UnitPrice = Guard.NotNegative(unitPrice, nameof(unitPrice));
        Quantity = quantity > 0 ? quantity : throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        RecalculateLineTotal();
    }

    public void ChangeQuantity(int quantity)
    {
        Quantity = quantity > 0 ? quantity : throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        RecalculateLineTotal();
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        DiscountAmount = Guard.NotNegative(discountAmount, nameof(discountAmount));
        RecalculateLineTotal();
    }

    public void ApplyTax(decimal taxAmount)
    {
        TaxAmount = Guard.NotNegative(taxAmount, nameof(taxAmount));
        RecalculateLineTotal();
    }

    public void RecalculateLineTotal()
    {
        LineTotal = (UnitPrice * Quantity) - DiscountAmount + TaxAmount;
    }
}
