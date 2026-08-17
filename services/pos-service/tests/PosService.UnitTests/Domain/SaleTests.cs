using FluentAssertions;
using PosService.Domain.Sales;
using Xunit;

namespace PosService.UnitTests.Domain;

public class SaleTests
{
    private static Sale MakeDraftSale() =>
        new("TST-20260816-0001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    private static SaleItem MakeItem(Guid saleId, decimal unitPrice = 100m, int quantity = 1) =>
        new(saleId, Guid.NewGuid(), "Widget", "WID-001", unitPrice, quantity);

    [Fact]
    public void CreateSale_WithValidData_ShouldHaveDraftStatus()
    {
        var sale = MakeDraftSale();

        sale.Status.Should().Be(SaleStatus.Draft);
        sale.SaleNumber.Should().Be("TST-20260816-0001");
        sale.Items.Should().BeEmpty();
        sale.Payments.Should().BeEmpty();
        sale.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public void RecalculateTotals_ShouldSumItemsCorrectly()
    {
        var sale = MakeDraftSale();
        sale.Items.Add(new SaleItem(sale.Id, Guid.NewGuid(), "Widget", "WID-001", 100m, 2));
        sale.Items.Add(new SaleItem(sale.Id, Guid.NewGuid(), "Gadget", "GAD-001", 50m, 1));

        sale.RecalculateTotals();

        sale.SubtotalAmount.Should().Be(250m);
        sale.TotalAmount.Should().Be(250m);
    }

    [Fact]
    public void RecalculateTotals_WithDiscountAndTax_ShouldApplyBoth()
    {
        var sale = MakeDraftSale();
        var item = new SaleItem(sale.Id, Guid.NewGuid(), "Widget", "WID-001", 100m, 1);
        item.ApplyDiscount(10m);
        item.ApplyTax(5m);
        sale.Items.Add(item);

        sale.RecalculateTotals();

        sale.SubtotalAmount.Should().Be(100m);
        sale.DiscountAmount.Should().Be(10m);
        sale.TaxAmount.Should().Be(5m);
        sale.TotalAmount.Should().Be(95m); // 100 - 10 + 5
    }

    [Fact]
    public void Complete_WithValidPayment_ShouldSetStatusAndCalculateChange()
    {
        var sale = MakeDraftSale();
        sale.Items.Add(MakeItem(sale.Id, 100m, 1));
        sale.RecalculateTotals();

        sale.Complete(120m);

        sale.Status.Should().Be(SaleStatus.Completed);
        sale.PaidAmount.Should().Be(120m);
        sale.ChangeAmount.Should().Be(20m);
    }

    [Fact]
    public void Complete_WithExactPayment_ShouldHaveZeroChange()
    {
        var sale = MakeDraftSale();
        sale.Items.Add(MakeItem(sale.Id, 100m, 1));
        sale.RecalculateTotals();

        sale.Complete(100m);

        sale.Status.Should().Be(SaleStatus.Completed);
        sale.ChangeAmount.Should().Be(0m);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var sale = MakeDraftSale();
        sale.Items.Add(MakeItem(sale.Id));
        sale.RecalculateTotals();
        sale.Complete(100m);

        Action act = () => sale.Complete(100m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*cannot be completed from status*");
    }

    [Fact]
    public void Complete_WithNoItems_ShouldThrow()
    {
        var sale = MakeDraftSale();

        Action act = () => sale.Complete(100m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*no line items*");
    }

    [Fact]
    public void Void_WithValidReason_ShouldSetStatusAndReason()
    {
        var sale = MakeDraftSale();

        sale.Void("Customer changed mind");

        sale.Status.Should().Be(SaleStatus.Voided);
        sale.VoidReason.Should().Be("Customer changed mind");
    }

    [Fact]
    public void Void_WhenAlreadyVoided_ShouldThrow()
    {
        var sale = MakeDraftSale();
        sale.Void("first reason");

        Action act = () => sale.Void("second reason");

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already voided*");
    }

    [Fact]
    public void Void_WithEmptyReason_ShouldThrow()
    {
        var sale = MakeDraftSale();

        Action act = () => sale.Void(string.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
