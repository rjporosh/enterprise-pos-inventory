using FluentAssertions;
using PosService.Domain.Sales;
using Xunit;

namespace PosService.UnitTests.Domain;

public class SaleItemTests
{
    [Fact]
    public void CreateSaleItem_WithValidData_ShouldCalculateLineTotal()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", "WID-001", 25m, 4);

        item.LineTotal.Should().Be(100m);
        item.Quantity.Should().Be(4);
        item.UnitPrice.Should().Be(25m);
        item.DiscountAmount.Should().Be(0m);
        item.TaxAmount.Should().Be(0m);
    }

    [Fact]
    public void CreateSaleItem_WithEmptyProductName_ShouldThrow()
    {
        Action act = () => new SaleItem(Guid.NewGuid(), Guid.NewGuid(), string.Empty, "WID-001", 25m, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateSaleItem_WithZeroQuantity_ShouldThrow()
    {
        Action act = () => new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", "WID-001", 25m, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateSaleItem_WithNegativePrice_ShouldThrow()
    {
        Action act = () => new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", "WID-001", -1m, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ApplyDiscount_ShouldReduceLineTotal()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", "WID-001", 100m, 1);

        item.ApplyDiscount(15m);

        item.DiscountAmount.Should().Be(15m);
        item.LineTotal.Should().Be(85m);
    }

    [Fact]
    public void ApplyTax_ShouldIncreaseLineTotal()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", "WID-001", 100m, 1);

        item.ApplyTax(10m);

        item.TaxAmount.Should().Be(10m);
        item.LineTotal.Should().Be(110m);
    }

    [Fact]
    public void ChangeQuantity_ShouldUpdateLineTotalProportionally()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", "WID-001", 50m, 2);

        item.ChangeQuantity(5);

        item.Quantity.Should().Be(5);
        item.LineTotal.Should().Be(250m);
    }

    [Fact]
    public void ChangeQuantity_ToZero_ShouldThrow()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", "WID-001", 50m, 2);

        Action act = () => item.ChangeQuantity(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
