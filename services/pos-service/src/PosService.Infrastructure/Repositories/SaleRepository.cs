using Microsoft.EntityFrameworkCore;
using PosService.Application.Sales.Repositories;
using PosService.Domain.Sales;
using PosService.Infrastructure.Persistence;

namespace PosService.Infrastructure.Repositories;

public class SaleRepository(PosDbContext context) : ISaleRepository
{
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Sales
            .IgnoreQueryFilters()
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken ct = default)
        => await context.Sales
            .IgnoreQueryFilters()
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, ct);

    public async Task<IReadOnlyList<Sale>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? storeId,
        Guid? cashierId,
        SaleStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct = default)
    {
        var query = BuildFilterQuery(storeId, cashierId, status, fromDate, toDate);

        return await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(Guid? storeId, Guid? cashierId, SaleStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
        => await BuildFilterQuery(storeId, cashierId, status, fromDate, toDate).CountAsync(ct);

    public async Task<int> GetNextSaleSequenceAsync(Guid storeId, DateTime date, CancellationToken ct = default)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var countToday = await context.Sales
            .IgnoreQueryFilters()
            .CountAsync(s => s.StoreId == storeId && s.SaleDate >= dayStart && s.SaleDate < dayEnd, ct);

        return countToday + 1;
    }

    public async Task<IReadOnlyList<Sale>> GetCompletedSalesForDateRangeAsync(Guid? storeId, DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct = default)
    {
        var query = context.Sales
            .IgnoreQueryFilters()
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= fromDateUtc && s.SaleDate < toDateUtc);

        if (storeId.HasValue)
            query = query.Where(s => s.StoreId == storeId.Value);

        return await query.ToListAsync(ct);
    }

    public void Add(Sale sale) => context.Sales.Add(sale);

    public void Update(Sale sale) => context.Sales.Update(sale);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);

    private IQueryable<Sale> BuildFilterQuery(Guid? storeId, Guid? cashierId, SaleStatus? status, DateTime? fromDate, DateTime? toDate)
    {
        var query = context.Sales.IgnoreQueryFilters().AsQueryable();

        if (storeId.HasValue) query = query.Where(s => s.StoreId == storeId.Value);
        if (cashierId.HasValue) query = query.Where(s => s.CashierId == cashierId.Value);
        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(s => s.SaleDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(s => s.SaleDate <= toDate.Value);

        return query;
    }
}
