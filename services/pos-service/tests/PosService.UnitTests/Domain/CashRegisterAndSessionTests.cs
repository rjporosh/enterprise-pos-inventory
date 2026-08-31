using FluentAssertions;
using PosService.Domain.Registers;
using Xunit;

namespace PosService.UnitTests.Domain;

public class CashRegisterTests
{
    [Fact]
    public void CreateCashRegister_WithValidData_ShouldSucceed()
    {
        var storeId = Guid.NewGuid();
        var register = new CashRegister("Register 1", "REG-001", storeId);

        register.Name.Should().Be("Register 1");
        register.Code.Should().Be("REG-001");
        register.StoreId.Should().Be(storeId);
        register.IsActive.Should().BeTrue();
        // Regression: see StoreTests.CreateStore_ShouldAssignANonEmptyId.
        register.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateCashRegister_WithEmptyCode_ShouldThrow()
    {
        Action act = () => new CashRegister("Register 1", string.Empty, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }
}

public class CashSessionTests
{
    [Fact]
    public void OpenSession_WithValidOpeningBalance_ShouldSucceed()
    {
        var session = new CashSession(Guid.NewGuid(), Guid.NewGuid(), 100m);

        session.OpeningBalance.Should().Be(100m);
        session.Status.Should().Be(CashSessionStatus.Open);
        session.ClosedAt.Should().BeNull();
    }

    [Fact]
    public void OpenSession_WithNegativeOpeningBalance_ShouldThrow()
    {
        Action act = () => new CashSession(Guid.NewGuid(), Guid.NewGuid(), -1m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CloseSession_ShouldComputeVarianceAndSetClosed()
    {
        var session = new CashSession(Guid.NewGuid(), Guid.NewGuid(), 100m);

        session.Close(closingBalance: 250m, expectedBalance: 260m, notes: "short by 10");

        session.Status.Should().Be(CashSessionStatus.Closed);
        session.ClosingBalance.Should().Be(250m);
        session.ExpectedBalance.Should().Be(260m);
        session.Variance.Should().Be(-10m);
        session.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void CloseSession_WhenAlreadyClosed_ShouldThrow()
    {
        var session = new CashSession(Guid.NewGuid(), Guid.NewGuid(), 100m);
        session.Close(100m, 100m);

        Action act = () => session.Close(100m, 100m);

        act.Should().Throw<InvalidOperationException>();
    }
}
