using Microsoft.EntityFrameworkCore;
using PosService.Application.Cashiers;
using PosService.Domain.Cashiers;
using PosService.Infrastructure.Persistence;

namespace PosService.Infrastructure.Repositories;

public class CashierRepository(PosDbContext context) : ICashierRepository
{
    public async Task<IReadOnlyList<Cashier>> GetAllAsync(CancellationToken ct = default)
        => await context.Cashiers.IgnoreQueryFilters().ToListAsync(ct);

    public async Task<Cashier?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Cashiers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExistsActiveAsync(Guid id, CancellationToken ct = default)
        => await context.Cashiers.IgnoreQueryFilters().AnyAsync(c => c.Id == id && c.IsActive && !c.IsDeleted, ct);

    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = context.Cashiers.IgnoreQueryFilters().Where(c => c.Username == username);
        if (excludeId.HasValue) query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public void Add(Cashier cashier) => context.Cashiers.Add(cashier);

    public void Update(Cashier cashier) => context.Cashiers.Update(cashier);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
