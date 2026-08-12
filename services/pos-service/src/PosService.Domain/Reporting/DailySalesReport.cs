using PosService.Domain.Common;

namespace PosService.Domain.Reporting;

/// <summary>
/// A generated, immutable snapshot of one store's sales activity for one calendar day (UTC). One row per
/// (StoreId, ReportDate) — the job that generates these skips a date that already has a row, which is
/// what makes report generation idempotent across retries/restarts. Breakdown detail (payment methods,
/// top products) is stored as JSON since this is a reporting snapshot, not a normalized transactional
/// table that needs its own queryable rows.
/// </summary>
public class DailySalesReport : BaseEntity
{
    public Guid StoreId { get; set; }
    public DateOnly ReportDate { get; set; }

    public int TotalSalesCount { get; set; }
    public int VoidedSalesCount { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetRevenue { get; set; }

    public decimal CashCollected { get; set; }
    public decimal CardCollected { get; set; }
    public decimal MobileMoneyCollected { get; set; }
    public decimal OtherCollected { get; set; }

    /// <summary>JSON array of { productId, sku, productName, quantitySold, revenue }, top 10 by revenue.</summary>
    public string TopProductsJson { get; set; } = "[]";

    /// <summary>JSON array of { registerId, cashierId, openingBalance, closingBalance, variance }.</summary>
    public string CashSessionSummaryJson { get; set; } = "[]";

    public DateTime GeneratedAtUtc { get; set; }

    public DailySalesReport() { }

    public DailySalesReport(Guid storeId, DateOnly reportDate)
    {
        StoreId = storeId;
        ReportDate = reportDate;
        GeneratedAtUtc = DateTime.UtcNow;
    }
}
