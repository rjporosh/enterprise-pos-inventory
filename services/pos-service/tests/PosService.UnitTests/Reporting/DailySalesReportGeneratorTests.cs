using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PosService.Application.Reporting;
using PosService.Application.Sales.Repositories;
using PosService.Domain.Reporting;
using PosService.Domain.Sales;
using Xunit;

namespace PosService.UnitTests.Reporting;

public class DailySalesReportGeneratorTests
{
    private readonly Mock<ILogger<DailySalesReportGenerator>> _loggerMock = new();
    private readonly Mock<ISaleRepository> _saleRepositoryMock = new();
    private readonly Mock<IDailySalesReportRepository> _reportRepositoryMock = new();

    private DailySalesReportGenerator CreateSut() =>
        new(_loggerMock.Object, _saleRepositoryMock.Object, _reportRepositoryMock.Object);

    [Fact]
    public async Task GenerateIfMissingAsync_WhenReportAlreadyExists_ShouldSkipAndReturnFalse()
    {
        var storeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        _reportRepositoryMock.Setup(r => r.ExistsAsync(storeId, date, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut();
        var generated = await sut.GenerateIfMissingAsync(storeId, date, CancellationToken.None);

        generated.Should().BeFalse();
        _saleRepositoryMock.Verify(r => r.GetCompletedSalesForDateRangeAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _reportRepositoryMock.Verify(r => r.Add(It.IsAny<DailySalesReport>()), Times.Never);
    }

    [Fact]
    public async Task GenerateIfMissingAsync_WhenNoReportExists_ShouldAggregateAndSaveReport()
    {
        var storeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var sale = new Sale("ST-001-20260101-0001", storeId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var item = new SaleItem(sale.Id, Guid.NewGuid(), "Widget", "SKU-1", 10m, 2);
        sale.Items.Add(item);
        sale.RecalculateTotals();
        sale.Complete(20m);
        sale.Payments.Add(new Payment(sale.Id, PaymentMethodType.Cash, 20m));

        _reportRepositoryMock.Setup(r => r.ExistsAsync(storeId, date, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _saleRepositoryMock.Setup(r => r.GetCompletedSalesForDateRangeAsync(storeId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Sale> { sale });

        DailySalesReport? savedReport = null;
        _reportRepositoryMock.Setup(r => r.Add(It.IsAny<DailySalesReport>())).Callback<DailySalesReport>(r => savedReport = r);

        var sut = CreateSut();
        var generated = await sut.GenerateIfMissingAsync(storeId, date, CancellationToken.None);

        generated.Should().BeTrue();
        savedReport.Should().NotBeNull();
        savedReport!.TotalSalesCount.Should().Be(1);
        savedReport.NetRevenue.Should().Be(20m);
        savedReport.CashCollected.Should().Be(20m);
        savedReport.CardCollected.Should().Be(0m);
        _reportRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
