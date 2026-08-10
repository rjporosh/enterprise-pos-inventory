using Microsoft.EntityFrameworkCore;
using InventoryService.Domain.Products;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Application.Products.Repositories;
using SharedKernel;

namespace InventoryService.Infrastructure.Repositories;

public class ProductRepository(InventoryDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Products
            .IgnoreQueryFilters()
            .ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        return await context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Sku == sku, ct);
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        return await context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Barcode == barcode, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Products
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Id == id, ct);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = context.Products.IgnoreQueryFilters().Where(p => p.Sku == sku);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = context.Products.IgnoreQueryFilters().Where(p => p.Barcode == barcode);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId, Guid? brandId, bool? isActive, string? searchTerm, string sortBy, bool sortDescending, CancellationToken ct = default)
    {
        var query = context.Products.IgnoreQueryFilters().AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);
        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);
        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || p.Sku.ToLower().Contains(lowerSearch));
        }

        query = sortBy.ToLower() switch
        {
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "sku" => sortDescending ? query.OrderByDescending(p => p.Sku) : query.OrderBy(p => p.Sku),
            "price" => sortDescending ? query.OrderByDescending(p => p.SellingPrice) : query.OrderBy(p => p.SellingPrice),
            "createdat" => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(Guid? categoryId, Guid? brandId, bool? isActive, string? searchTerm, CancellationToken ct = default)
    {
        var query = context.Products.IgnoreQueryFilters().AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);
        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);
        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || p.Sku.ToLower().Contains(lowerSearch));
        }

        return await query.CountAsync(ct);
    }

    public void Add(Product product) => context.Products.Add(product);

    public void Update(Product product) => context.Products.Update(product);

    public void SoftDelete(Product product)
    {
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
