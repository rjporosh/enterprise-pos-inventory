using PosService.Domain.Reporting;

namespace PosService.Application.Reporting;

public interface IDailySalesReportRepository
{
    Task<bool> ExistsAsync(Guid storeId, DateOnly reportDate, CancellationToken ct = default);
    Task<DailySalesReport?> GetAsync(Guid storeId, DateOnly reportDate, CancellationToken ct = default);
    void Add(DailySalesReport report);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
