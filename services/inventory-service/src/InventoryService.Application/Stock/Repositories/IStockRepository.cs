using global::InventoryService.Domain.Stock;
using InventoryService.Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace InventoryService.Application.Stock;

public interface IStockRepository
{
    Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetAllAsync(CancellationToken ct = default);
    Task<global::InventoryService.Domain.Stock.Stock?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<global::InventoryService.Domain.Stock.Stock?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken ct = default);
    Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken ct = default);
    Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetLowStockAsync(CancellationToken ct = default);
    Task<IReadOnlyList<global::InventoryService.Domain.Stock.Stock>> GetOutOfStockAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsForProductWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken ct = default);
    void Add(global::InventoryService.Domain.Stock.Stock stock);
    void Update(global::InventoryService.Domain.Stock.Stock stock);
    void SoftDelete(global::InventoryService.Domain.Stock.Stock stock);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
