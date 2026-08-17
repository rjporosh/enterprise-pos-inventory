using Microsoft.EntityFrameworkCore;
using PosService.Application.Stores;
using PosService.Domain.Stores;
using PosService.Infrastructure.Persistence;

namespace PosService.Infrastructure.Repositories;

public class StoreRepository(PosDbContext context) : IStoreRepository
{
    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default)
        => await context.Stores.IgnoreQueryFilters().ToListAsync(ct);

    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Stores.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<bool> ExistsActiveAsync(Guid id, CancellationToken ct = default)
        => await context.Stores.IgnoreQueryFilters().AnyAsync(s => s.Id == id && s.IsActive && !s.IsDeleted, ct);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = context.Stores.IgnoreQueryFilters().Where(s => s.Code == code);
        if (excludeId.HasValue) query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public void Add(Store store) => context.Stores.Add(store);

    public void Update(Store store) => context.Stores.Update(store);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
