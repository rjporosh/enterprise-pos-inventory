using InventoryService.Domain.Warehouses;

namespace InventoryService.Application.Warehouses;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken ct = default);
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
