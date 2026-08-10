using InventoryService.Domain.Products;
using InventoryService.Domain.Warehouses;
using InventoryService.Domain.Common;

namespace InventoryService.Domain.Stock;

public class Stock : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public int QuantityOnHand { get; set; } = 0;
    public int QuantityReserved { get; set; } = 0;
    public int ReorderLevel { get; set; } = 0;
    public int MaxStockLevel { get; set; } = 0;
    public DateTime? LastRestockedAt { get; set; }

    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();
    private readonly List<StockMovement> _movements = new();

    public int AvailableQuantity => QuantityOnHand - QuantityReserved;

    public Stock() { }

    public Stock(Guid productId, Guid warehouseId, int reorderLevel = 0, int maxStockLevel = 0)
    {
        ProductId = productId == Guid.Empty ? throw new ArgumentException("ProductId cannot be empty.", nameof(productId)) : productId;
        WarehouseId = warehouseId == Guid.Empty ? throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId)) : warehouseId;
        ReorderLevel = SharedKernel.Guard.NotNegative(reorderLevel, nameof(reorderLevel));
        MaxStockLevel = SharedKernel.Guard.NotNegative(maxStockLevel, nameof(maxStockLevel));
    }

    public void UpdateSettings(int reorderLevel, int maxStockLevel)
    {
        ReorderLevel = SharedKernel.Guard.NotNegative(reorderLevel, nameof(reorderLevel));
        MaxStockLevel = SharedKernel.Guard.NotNegative(maxStockLevel, nameof(maxStockLevel));
    }

    public void AddMovement(StockMovement movement)
    {
        if (movement is null)
            throw new ArgumentNullException(nameof(movement));

        _movements.Add(movement);
        QuantityOnHand += movement.Quantity;

        if (movement.MovementType is StockMovementType.StockIn or StockMovementType.TransferIn or StockMovementType.Return)
        {
            LastRestockedAt = movement.CreatedAt;
        }
    }
}
