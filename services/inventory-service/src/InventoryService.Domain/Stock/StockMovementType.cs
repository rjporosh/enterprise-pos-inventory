namespace InventoryService.Domain.Stock;

public enum StockMovementType
{
    StockIn = 1,
    StockOut = 2,
    Adjustment = 3,
    TransferIn = 4,
    TransferOut = 5,
    Sale = 6,
    Return = 7
}
