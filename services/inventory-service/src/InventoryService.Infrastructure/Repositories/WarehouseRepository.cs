using InventoryService.Application.Warehouses;
using InventoryService.Domain.Warehouses;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class WarehouseRepository(InventoryDbContext context) : IWarehouseRepository
{
    public async Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken ct = default)
        => await context.Warehouses.IgnoreQueryFilters().FirstOrDefaultAsync(w => w.IsDefault && w.IsActive, ct);

    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Warehouses.IgnoreQueryFilters().FirstOrDefaultAsync(w => w.Id == id, ct);
}
