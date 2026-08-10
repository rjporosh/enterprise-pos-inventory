using FluentAssertions;
using InventoryService.Domain.Warehouses;
using Xunit;

namespace InventoryService.UnitTests.Domain;

public class WarehouseTests
{
    [Fact]
    public void CreateWarehouse_WithValidData_ShouldSucceed()
    {
        var warehouse = new Warehouse("Main Warehouse", "WH-001", "123 Main St");
        
        warehouse.Name.Should().Be("Main Warehouse");
        warehouse.Code.Should().Be("WH-001");
        warehouse.Address.Should().Be("123 Main St");
        warehouse.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void CreateWarehouse_DefaultsToNotDefault()
    {
        var warehouse = new Warehouse("Test Warehouse");
        
        warehouse.IsDefault.Should().BeFalse();
    }
}
