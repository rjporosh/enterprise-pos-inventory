using Microsoft.EntityFrameworkCore;
using PosService.Application.Customers;
using PosService.Domain.Customers;
using PosService.Infrastructure.Persistence;

namespace PosService.Infrastructure.Repositories;

public class CustomerRepository(PosDbContext context) : ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
        => await context.Customers.IgnoreQueryFilters().ToListAsync(ct);

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExistsActiveAsync(Guid id, CancellationToken ct = default)
        => await context.Customers.IgnoreQueryFilters().AnyAsync(c => c.Id == id && c.IsActive && !c.IsDeleted, ct);

    public void Add(Customer customer) => context.Customers.Add(customer);

    public void Update(Customer customer) => context.Customers.Update(customer);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
