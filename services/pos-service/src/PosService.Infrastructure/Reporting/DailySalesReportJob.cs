using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PosService.Application.Reporting;
using PosService.Application.Stores;
using PosService.Domain.Stores;

namespace PosService.Infrastructure.Reporting;

/// <summary>
/// Generates the previous day's DailySalesReport for every store, once per day at UTC midnight.
/// - Idempotent: DailySalesReportGenerator no-ops if a report already exists for that store/date.
/// - Recovery after restart: on startup, walks back up to CatchUpDays looking for any store/date
///   combination missing a report and generates it, so a period of downtime doesn't lose reports.
/// - Retry: a failure for one store/date is logged and does not stop the other stores or the next
///   scheduled run from proceeding.
/// </summary>
public class DailySalesReportJob(
    IServiceScopeFactory scopeFactory,
    ILogger<DailySalesReportJob> logger) : BackgroundService
{
    private const int CatchUpDays = 7;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunCatchUpAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var reportDate = DateOnly.FromDateTime(nextMidnight.AddDays(-1));
            await GenerateForAllStoresAsync(reportDate, stoppingToken);
        }
    }

    private async Task RunCatchUpAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        for (var offset = 1; offset <= CatchUpDays; offset++)
        {
            var date = today.AddDays(-offset);

            try
            {
                await GenerateForAllStoresAsync(date, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Catch-up report generation failed for {ReportDate}; will retry on next restart", date);
            }
        }
    }

    private async Task GenerateForAllStoresAsync(DateOnly reportDate, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var storeRepository = scope.ServiceProvider.GetRequiredService<IStoreRepository>();
        var generator = scope.ServiceProvider.GetRequiredService<IDailySalesReportGenerator>();

        IReadOnlyList<Store> stores;
        try
        {
            stores = await storeRepository.GetAllAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not load store list for daily report generation on {ReportDate}", reportDate);
            return;
        }

        foreach (var store in stores)
        {
            try
            {
                await generator.GenerateIfMissingAsync(store.Id, reportDate, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate daily sales report for store {StoreId} on {ReportDate}", store.Id, reportDate);
            }
        }
    }
}
