namespace InventoryService.Application.Stock;

public record StockDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid WarehouseId,
    string WarehouseName,
    string WarehouseCode,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    int ReorderLevel,
    int MaxStockLevel,
    DateTime? LastRestockedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record StockListItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid WarehouseId,
    string WarehouseName,
    string WarehouseCode,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    int ReorderLevel,
    bool IsLowStock,
    DateTime? LastRestockedAt);

public record CreateStockRequest(
    Guid ProductId,
    Guid WarehouseId,
    int InitialQuantity,
    int ReorderLevel,
    int MaxStockLevel,
    decimal? UnitCost);

public record UpdateStockRequest(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    int ReorderLevel,
    int MaxStockLevel);

public record StockMovementDto(
    Guid Id,
    Guid StockId,
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseName,
    global::InventoryService.Domain.Stock.StockMovementType MovementType,
    int Quantity,
    int BalanceAfter,
    decimal? UnitCost,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Notes,
    DateTime CreatedAt);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize);
