using PosService.Domain.Stores;

namespace PosService.Application.Stores;

public interface IStoreRepository
{
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default);
    Task<Store?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsActiveAsync(Guid id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    void Add(Store store);
    void Update(Store store);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
