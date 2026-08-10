using Microsoft.EntityFrameworkCore;
using global::InventoryService.Domain.Stock;
using InventoryService.Infrastructure.Persistence;
using SharedKernel;

namespace InventoryService.Infrastructure.Repositories;

public class StockRepository(InventoryDbContext context) : global::InventoryService.Application.Stock.IStockRepository
{
    public async Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .ToListAsync(ct);
    }

    public async Task<global::InventoryService.Domain.Stock.Stock?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<global::InventoryService.Domain.Stock.Stock?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId, ct);
    }

    public async Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .Include(s => s.Warehouse)
            .Where(s => s.ProductId == productId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .Include(s => s.Product)
            .Where(s => s.WarehouseId == warehouseId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetLowStockAsync(CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => !s.IsDeleted && s.QuantityOnHand <= s.ReorderLevel && s.QuantityOnHand > 0)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetOutOfStockAsync(CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => !s.IsDeleted && s.QuantityOnHand <= 0)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Id == id, ct);
    }

    public async Task<bool> ExistsForProductWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken ct = default)
    {
        return await context.Stocks
            .IgnoreQueryFilters()
            .AnyAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId, ct);
    }

    public void Add(global::InventoryService.Domain.Stock.Stock stock) => context.Stocks.Add(stock);

    public void Update(global::InventoryService.Domain.Stock.Stock stock) => context.Stocks.Update(stock);

    public void SoftDelete(global::InventoryService.Domain.Stock.Stock stock)
    {
        stock.IsDeleted = true;
        stock.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
