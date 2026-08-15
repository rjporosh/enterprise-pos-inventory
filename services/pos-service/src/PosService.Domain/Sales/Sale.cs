using PosService.Domain.Cashiers;
using PosService.Domain.Common;
using PosService.Domain.Customers;
using PosService.Domain.Registers;
using PosService.Domain.Stores;
using SharedKernel;
using BaseEntity = PosService.Domain.Common.BaseEntity;

namespace PosService.Domain.Sales;

/// <summary>
/// A checkout transaction. Aggregate root for SaleItem and Payment rows (each is its own table,
/// linked by SaleId — not owned EF types — matching the Stock/StockMovement convention used by
/// the Inventory service).
/// </summary>
public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public Guid RegisterId { get; set; }
    public Guid CashierId { get; set; }
    public Guid CashSessionId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime SaleDate { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Draft;
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? VoidReason { get; set; }
    public string? Notes { get; set; }

    public Store Store { get; set; } = null!;
    public CashRegister Register { get; set; } = null!;
    public Cashier Cashier { get; set; } = null!;
    public CashSession CashSession { get; set; } = null!;
    public Customer? Customer { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public Sale() { }

    public Sale(string saleNumber, Guid storeId, Guid registerId, Guid cashierId, Guid cashSessionId, Guid? customerId = null)
    {
        SaleNumber = Guard.NotNullOrEmpty(saleNumber, nameof(saleNumber));
        StoreId = storeId;
        RegisterId = registerId;
        CashierId = cashierId;
        CashSessionId = cashSessionId;
        CustomerId = customerId;
        SaleDate = DateTime.UtcNow;
        Status = SaleStatus.Draft;
    }

    /// <summary>Recomputes Subtotal/Tax/Total from the current in-memory Items collection.</summary>
    public void RecalculateTotals()
    {
        SubtotalAmount = Items.Sum(i => i.UnitPrice * i.Quantity);
        DiscountAmount = Items.Sum(i => i.DiscountAmount);
        TaxAmount = Items.Sum(i => i.TaxAmount);
        TotalAmount = SubtotalAmount - DiscountAmount + TaxAmount;
    }

    public void Complete(decimal paidAmount)
    {
        if (Status != SaleStatus.Draft)
        {
            throw new InvalidOperationException($"Sale {SaleNumber} cannot be completed from status {Status}.");
        }

        if (Items.Count == 0)
        {
            throw new InvalidOperationException($"Sale {SaleNumber} has no line items.");
        }

        PaidAmount = paidAmount;
        ChangeAmount = paidAmount > TotalAmount ? paidAmount - TotalAmount : 0;
        Status = SaleStatus.Completed;
    }

    public void Void(string reason)
    {
        if (Status == SaleStatus.Voided)
        {
            throw new InvalidOperationException($"Sale {SaleNumber} is already voided.");
        }

        VoidReason = Guard.NotNullOrEmpty(reason, nameof(reason));
        Status = SaleStatus.Voided;
    }
}
