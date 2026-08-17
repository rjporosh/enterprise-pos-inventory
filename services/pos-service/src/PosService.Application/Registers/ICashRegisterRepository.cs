using PosService.Domain.Registers;

namespace PosService.Application.Registers;

public interface ICashRegisterRepository
{
    Task<IReadOnlyList<CashRegister>> GetAllAsync(CancellationToken ct = default);
    Task<CashRegister?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsActiveAsync(Guid id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    void Add(CashRegister register);
    void Update(CashRegister register);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface ICashSessionRepository
{
    Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CashSession?> GetOpenSessionByRegisterIdAsync(Guid registerId, CancellationToken ct = default);
    Task<bool> HasOpenSessionAsync(Guid registerId, CancellationToken ct = default);
    void Add(CashSession session);
    void Update(CashSession session);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
