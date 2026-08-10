using InventoryService.Domain.Common;
using InventoryService.Domain.Products;

namespace InventoryService.Domain.Stock;

public class StockMovement : BaseEntity
{
    public Guid StockId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int BalanceAfter { get; set; }
    public decimal? UnitCost { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }

    public StockMovement() { }

    public StockMovement(
        Guid stockId,
        Guid productId,
        Guid warehouseId,
        StockMovementType movementType,
        int quantity,
        int balanceAfter,
        decimal? unitCost = null,
        string? referenceType = null,
        Guid? referenceId = null,
        string? notes = null)
    {
        StockId = stockId == Guid.Empty ? throw new ArgumentException("StockId cannot be empty.", nameof(stockId)) : stockId;
        ProductId = productId == Guid.Empty ? throw new ArgumentException("ProductId cannot be empty.", nameof(productId)) : productId;
        WarehouseId = warehouseId == Guid.Empty ? throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId)) : warehouseId;
        MovementType = movementType;
        Quantity = quantity == 0 ? throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must not be zero.") : quantity;
        BalanceAfter = SharedKernel.Guard.NotNegative(balanceAfter, nameof(balanceAfter));
        UnitCost = unitCost;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Notes = notes;
    }
}
