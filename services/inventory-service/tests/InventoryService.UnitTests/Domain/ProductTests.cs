using FluentAssertions;
using InventoryService.Domain.Products;
using Xunit;

namespace InventoryService.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void CreateProduct_WithValidData_ShouldSucceed()
    {
        var product = new Product(
            name: "Laptop",
            sku: "LAP-001",
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            unitId: Guid.NewGuid(),
            costPrice: 50000,
            sellingPrice: 60000);

        product.Name.Should().Be("Laptop");
        product.Sku.Should().Be("LAP-001");
        product.CostPrice.Should().Be(50000);
        product.SellingPrice.Should().Be(60000);
    }

    [Fact]
    public void CreateProduct_WithNegativePrice_ShouldThrow()
    {
        Action act = () => new Product(
            name: "Test",
            sku: "T-001",
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            unitId: Guid.NewGuid(),
            costPrice: -100,
            sellingPrice: 100);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdatePrice_WithValidPrices_ShouldUpdate()
    {
        var product = new Product("Test", "T-001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100, 200);
        product.UpdatePrice(150, 250);

        product.CostPrice.Should().Be(150);
        product.SellingPrice.Should().Be(250);
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ShouldThrow()
    {
        var product = new Product("Test", "T-001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100, 200);
        Action act = () => product.UpdatePrice(-50, 200);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
