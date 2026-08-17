using Microsoft.EntityFrameworkCore;
using PosService.Application.Reporting;
using PosService.Domain.Reporting;
using PosService.Infrastructure.Persistence;

namespace PosService.Infrastructure.Repositories;

public class DailySalesReportRepository(PosDbContext context) : IDailySalesReportRepository
{
    public async Task<bool> ExistsAsync(Guid storeId, DateOnly reportDate, CancellationToken ct = default)
        => await context.DailySalesReports.IgnoreQueryFilters()
            .AnyAsync(r => r.StoreId == storeId && r.ReportDate == reportDate, ct);

    public async Task<DailySalesReport?> GetAsync(Guid storeId, DateOnly reportDate, CancellationToken ct = default)
        => await context.DailySalesReports.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.StoreId == storeId && r.ReportDate == reportDate, ct);

    public void Add(DailySalesReport report) => context.DailySalesReports.Add(report);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
