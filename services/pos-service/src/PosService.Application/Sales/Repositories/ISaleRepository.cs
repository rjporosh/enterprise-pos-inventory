using PosService.Domain.Sales;

namespace PosService.Application.Sales.Repositories;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Sale>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? storeId,
        Guid? cashierId,
        SaleStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid? storeId, Guid? cashierId, SaleStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<int> GetNextSaleSequenceAsync(Guid storeId, DateTime date, CancellationToken ct = default);

    /// <summary>Sum of TotalAmount for Completed sales in a date range, used by the daily reporting job.</summary>
    Task<IReadOnlyList<Sale>> GetCompletedSalesForDateRangeAsync(Guid? storeId, DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct = default);

    void Add(Sale sale);
    void Update(Sale sale);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
