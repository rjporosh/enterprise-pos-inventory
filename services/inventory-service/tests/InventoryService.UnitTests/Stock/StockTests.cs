using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InventoryService.UnitTests.Stock;

public class StockTests
{
    [Fact]
    public void Stock_Constructor_WithValidData_ShouldCreateInstance()
    {
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        var stock = new global::InventoryService.Domain.Stock.Stock(productId, warehouseId, 5, 100);

        stock.ProductId.Should().Be(productId);
        stock.WarehouseId.Should().Be(warehouseId);
        stock.QuantityOnHand.Should().Be(0);
        stock.QuantityReserved.Should().Be(0);
        stock.ReorderLevel.Should().Be(5);
        stock.MaxStockLevel.Should().Be(100);
        stock.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public void Stock_Constructor_WithEmptyProductId_ShouldThrow()
    {
        Action act = () => new global::InventoryService.Domain.Stock.Stock(Guid.Empty, Guid.NewGuid());
        act.Should().Throw<ArgumentException>().WithMessage("*ProductId*");
    }

    [Fact]
    public void Stock_AddMovement_ShouldUpdateQuantityOnHand()
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(Guid.NewGuid(), Guid.NewGuid());
        stock.AddMovement(new global::InventoryService.Domain.Stock.StockMovement(
            stock.Id, stock.ProductId, stock.WarehouseId,
            global::InventoryService.Domain.Stock.StockMovementType.StockIn, 50, 50));

        stock.QuantityOnHand.Should().Be(50);
        stock.AvailableQuantity.Should().Be(50);
    }

    [Fact]
    public void Stock_AvailableQuantity_ShouldBeOnHandMinusReserved()
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(Guid.NewGuid(), Guid.NewGuid(), 0, 0);
        stock.AddMovement(new global::InventoryService.Domain.Stock.StockMovement(
            stock.Id, stock.ProductId, stock.WarehouseId,
            global::InventoryService.Domain.Stock.StockMovementType.StockIn, 100, 100));

        stock.QuantityReserved = 30;

        stock.AvailableQuantity.Should().Be(70);
    }

    [Fact]
    public void StockMovement_Constructor_WithZeroQuantity_ShouldThrow()
    {
        Action act = () => new global::InventoryService.Domain.Stock.StockMovement(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            global::InventoryService.Domain.Stock.StockMovementType.StockIn, 0, 0);

        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*quantity*");
    }
}
