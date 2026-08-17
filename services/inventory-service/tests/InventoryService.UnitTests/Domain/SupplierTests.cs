using FluentAssertions;
using InventoryService.Domain.Suppliers;
using Xunit;

namespace InventoryService.UnitTests.Domain;

public class SupplierTests
{
    [Fact]
    public void CreateSupplier_WithValidData_ShouldSucceed()
    {
        var supplier = new Supplier("Acme Corp", "John Doe", "john@acme.com", "+1234567890");
        
        supplier.Name.Should().Be("Acme Corp");
        supplier.ContactName.Should().Be("John Doe");
        supplier.Email.Should().Be("john@acme.com");
        supplier.Phone.Should().Be("+1234567890");
    }

    [Fact]
    public void UpdateContact_ShouldUpdateEmailAndPhone()
    {
        var supplier = new Supplier("Acme Corp");
        supplier.UpdateContact("new@acme.com", "+9999999999");
        
        supplier.Email.Should().Be("new@acme.com");
        supplier.Phone.Should().Be("+9999999999");
    }
}
