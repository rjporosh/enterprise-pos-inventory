using Microsoft.EntityFrameworkCore;
using PosService.Application.Registers;
using PosService.Domain.Registers;
using PosService.Infrastructure.Persistence;

namespace PosService.Infrastructure.Repositories;

public class CashRegisterRepository(PosDbContext context) : ICashRegisterRepository
{
    public async Task<IReadOnlyList<CashRegister>> GetAllAsync(CancellationToken ct = default)
        => await context.CashRegisters.IgnoreQueryFilters().ToListAsync(ct);

    public async Task<CashRegister?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.CashRegisters.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<bool> ExistsActiveAsync(Guid id, CancellationToken ct = default)
        => await context.CashRegisters.IgnoreQueryFilters().AnyAsync(r => r.Id == id && r.IsActive && !r.IsDeleted, ct);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = context.CashRegisters.IgnoreQueryFilters().Where(r => r.Code == code);
        if (excludeId.HasValue) query = query.Where(r => r.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public void Add(CashRegister register) => context.CashRegisters.Add(register);

    public void Update(CashRegister register) => context.CashRegisters.Update(register);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}

public class CashSessionRepository(PosDbContext context) : ICashSessionRepository
{
    public async Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.CashSessions.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<CashSession?> GetOpenSessionByRegisterIdAsync(Guid registerId, CancellationToken ct = default)
        => await context.CashSessions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.RegisterId == registerId && s.Status == CashSessionStatus.Open, ct);

    public async Task<bool> HasOpenSessionAsync(Guid registerId, CancellationToken ct = default)
        => await context.CashSessions.IgnoreQueryFilters()
            .AnyAsync(s => s.RegisterId == registerId && s.Status == CashSessionStatus.Open, ct);

    public void Add(CashSession session) => context.CashSessions.Add(session);

    public void Update(CashSession session) => context.CashSessions.Update(session);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
