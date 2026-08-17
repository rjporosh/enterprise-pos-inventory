using MediatR;
using SharedKernel;

namespace PosService.Application.Reporting;

public record DailySalesReportDto(
    Guid StoreId,
    DateOnly ReportDate,
    int TotalSalesCount,
    int VoidedSalesCount,
    decimal GrossRevenue,
    decimal TotalDiscount,
    decimal TotalTax,
    decimal NetRevenue,
    decimal CashCollected,
    decimal CardCollected,
    decimal MobileMoneyCollected,
    decimal OtherCollected,
    string TopProductsJson,
    string CashSessionSummaryJson,
    DateTime GeneratedAtUtc);

public record GetDailySalesReportQuery(Guid StoreId, DateOnly ReportDate) : IRequest<Result<DailySalesReportDto>>;

public class GetDailySalesReportHandler(IDailySalesReportRepository repository) : IRequestHandler<GetDailySalesReportQuery, Result<DailySalesReportDto>>
{
    public async Task<Result<DailySalesReportDto>> Handle(GetDailySalesReportQuery query, CancellationToken ct)
    {
        var report = await repository.GetAsync(query.StoreId, query.ReportDate, ct);

        if (report is null)
        {
            return Result<DailySalesReportDto>.Failure(new Error("REPORT_NOT_FOUND", $"No report found for store '{query.StoreId}' on {query.ReportDate}."));
        }

        return new DailySalesReportDto(
            report.StoreId, report.ReportDate, report.TotalSalesCount, report.VoidedSalesCount,
            report.GrossRevenue, report.TotalDiscount, report.TotalTax, report.NetRevenue,
            report.CashCollected, report.CardCollected, report.MobileMoneyCollected, report.OtherCollected,
            report.TopProductsJson, report.CashSessionSummaryJson, report.GeneratedAtUtc);
    }
}
