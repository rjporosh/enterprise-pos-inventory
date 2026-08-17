using System.Text.Json;
using Microsoft.Extensions.Logging;
using PosService.Application.Sales.Repositories;
using PosService.Domain.Reporting;
using PosService.Domain.Sales;

namespace PosService.Application.Reporting;

public interface IDailySalesReportGenerator
{
    /// <summary>
    /// Generates (and persists) the report for one store/date if it doesn't already exist. Returns
    /// false without doing any work if a report for that store/date is already present — this is what
    /// makes calling it repeatedly (retries, catch-up after restart, redundant scheduler ticks) safe.
    /// </summary>
    Task<bool> GenerateIfMissingAsync(Guid storeId, DateOnly reportDate, CancellationToken ct = default);
}

public record TopProductLine(Guid ProductId, string Sku, string ProductName, int QuantitySold, decimal Revenue);

public record CashSessionSummaryLine(Guid RegisterId, Guid CashierId, decimal OpeningBalance, decimal? ClosingBalance, decimal? Variance);

public class DailySalesReportGenerator(
    ILogger<DailySalesReportGenerator> logger,
    ISaleRepository saleRepository,
    IDailySalesReportRepository reportRepository) : IDailySalesReportGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<bool> GenerateIfMissingAsync(Guid storeId, DateOnly reportDate, CancellationToken ct = default)
    {
        if (await reportRepository.ExistsAsync(storeId, reportDate, ct))
        {
            logger.LogDebug("Daily sales report for store {StoreId} on {ReportDate} already exists; skipping", storeId, reportDate);
            return false;
        }

        var dayStartUtc = reportDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEndUtc = dayStartUtc.AddDays(1);

        var completedSales = await saleRepository.GetCompletedSalesForDateRangeAsync(storeId, dayStartUtc, dayEndUtc, ct);

        var report = new DailySalesReport(storeId, reportDate)
        {
            TotalSalesCount = completedSales.Count,
            GrossRevenue = completedSales.Sum(s => s.SubtotalAmount),
            TotalDiscount = completedSales.Sum(s => s.DiscountAmount),
            TotalTax = completedSales.Sum(s => s.TaxAmount),
            NetRevenue = completedSales.Sum(s => s.TotalAmount),
            CashCollected = SumPayments(completedSales, PaymentMethodType.Cash),
            CardCollected = SumPayments(completedSales, PaymentMethodType.Card),
            MobileMoneyCollected = SumPayments(completedSales, PaymentMethodType.MobileMoney),
            OtherCollected = SumPayments(completedSales, PaymentMethodType.StoreCredit) + SumPayments(completedSales, PaymentMethodType.Other),
        };

        var topProducts = completedSales
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, i.Sku, i.ProductName })
            .Select(g => new TopProductLine(g.Key.ProductId, g.Key.Sku, g.Key.ProductName, g.Sum(i => i.Quantity), g.Sum(i => i.LineTotal)))
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();

        report.TopProductsJson = JsonSerializer.Serialize(topProducts, JsonOptions);

        // Voided count for the day is informational; a voided sale is excluded from GetCompletedSalesForDateRangeAsync
        // by definition (that query only returns Status == Completed), so it's tracked separately here.
        // Left at 0 pending a dedicated repository query — see handover for follow-up.
        report.VoidedSalesCount = 0;

        report.CashSessionSummaryJson = JsonSerializer.Serialize(Array.Empty<CashSessionSummaryLine>(), JsonOptions);
        // Cash-session-level detail (per-register opening/closing/variance) is left empty pending a
        // date-ranged query on ICashSessionRepository — see handover for this follow-up.

        reportRepository.Add(report);
        await reportRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Generated daily sales report for store {StoreId} on {ReportDate}: {SalesCount} sales, net revenue {NetRevenue:0.00}",
            storeId, reportDate, report.TotalSalesCount, report.NetRevenue);

        return true;
    }

    private static decimal SumPayments(IReadOnlyList<Sale> sales, PaymentMethodType method)
        => sales.SelectMany(s => s.Payments).Where(p => p.Method == method).Sum(p => p.Amount);
}
