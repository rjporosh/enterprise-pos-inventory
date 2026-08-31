using FluentAssertions;
using PosService.Domain.Stores;
using Xunit;

namespace PosService.UnitTests.Domain;

public class StoreTests
{
    [Fact]
    public void CreateStore_WithValidData_ShouldSucceed()
    {
        var store = new Store("Downtown Store", "ST-001");

        store.Name.Should().Be("Downtown Store");
        store.Code.Should().Be("ST-001");
        store.Currency.Should().Be("USD");
        store.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateStore_ShouldAssignANonEmptyId()
    {
        // Regression test: PosService.Domain.Common.BaseEntity's constructor never called
        // Id = Guid.NewGuid() (unlike InventoryService's equivalent and SharedKernel.BaseEntity),
        // so every entity built on it -- Store, CashRegister, Sale, CashSession, Cashier,
        // Customer, Payment -- was persisted with Id = Guid.Empty. A second insert of any entity
        // type then violated the primary key. Found 2026-08-31 while adding Store/Register CRUD.
        var store1 = new Store("Downtown Store", "ST-001");
        var store2 = new Store("Uptown Store", "ST-002");

        store1.Id.Should().NotBe(Guid.Empty);
        store2.Id.Should().NotBe(Guid.Empty);
        store1.Id.Should().NotBe(store2.Id);
    }

    [Fact]
    public void CreateStore_WithEmptyName_ShouldThrow()
    {
        Action act = () => new Store(string.Empty, "ST-001");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void CreateStore_WithEmptyCode_ShouldThrow()
    {
        Action act = () => new Store("Downtown Store", string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RenameStore_WithValidName_ShouldUpdateName()
    {
        var store = new Store("OldName", "ST-001");
        store.Rename("NewName");

        store.Name.Should().Be("NewName");
    }

    [Fact]
    public void DeactivateStore_ShouldSetIsActiveFalse()
    {
        var store = new Store("Downtown Store", "ST-001");
        store.Deactivate();

        store.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ActivateStore_ShouldSetIsActiveTrue()
    {
        var store = new Store("Downtown Store", "ST-001");
        store.Deactivate();
        store.Activate();

        store.IsActive.Should().BeTrue();
    }
}
