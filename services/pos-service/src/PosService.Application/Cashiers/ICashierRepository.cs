using PosService.Domain.Cashiers;

namespace PosService.Application.Cashiers;

public interface ICashierRepository
{
    Task<IReadOnlyList<Cashier>> GetAllAsync(CancellationToken ct = default);
    Task<Cashier?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Cashier?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsActiveAsync(Guid id, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, Guid? excludeId = null, CancellationToken ct = default);
    void Add(Cashier cashier);
    void Update(Cashier cashier);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
