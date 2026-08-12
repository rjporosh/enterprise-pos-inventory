using FluentAssertions;
using PosService.Domain.Cashiers;
using Xunit;

namespace PosService.UnitTests.Domain;

public class CashierTests
{
    [Fact]
    public void CreateCashier_WithValidData_ShouldSucceed()
    {
        var storeId = Guid.NewGuid();
        var cashier = new Cashier("Jane Doe", "jane.doe", storeId);

        cashier.FullName.Should().Be("Jane Doe");
        cashier.Username.Should().Be("jane.doe");
        cashier.StoreId.Should().Be(storeId);
        cashier.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateCashier_WithEmptyFullName_ShouldThrow()
    {
        Action act = () => new Cashier(string.Empty, "jane.doe", Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateCashier_WithEmptyUsername_ShouldThrow()
    {
        Action act = () => new Cashier("Jane Doe", string.Empty, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeactivateCashier_ShouldSetIsActiveFalse()
    {
        var cashier = new Cashier("Jane Doe", "jane.doe", Guid.NewGuid());
        cashier.Deactivate();

        cashier.IsActive.Should().BeFalse();
    }
}
