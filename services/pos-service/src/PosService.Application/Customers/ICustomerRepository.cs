using PosService.Domain.Customers;

namespace PosService.Application.Customers;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsActiveAsync(Guid id, CancellationToken ct = default);
    void Add(Customer customer);
    void Update(Customer customer);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
