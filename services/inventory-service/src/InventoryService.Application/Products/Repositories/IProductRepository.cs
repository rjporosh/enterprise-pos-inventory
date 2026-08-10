using InventoryService.Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace InventoryService.Application.Products.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId, Guid? brandId, bool? isActive, string? searchTerm, string sortBy, bool sortDescending, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid? categoryId, Guid? brandId, bool? isActive, string? searchTerm, CancellationToken ct = default);
    void Add(Product product);
    void Update(Product product);
    void SoftDelete(Product product);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
