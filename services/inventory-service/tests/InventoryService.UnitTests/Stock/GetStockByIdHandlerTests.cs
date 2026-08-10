using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using global::InventoryService.Domain.Stock;
using global::InventoryService.Domain.Products;
using global::InventoryService.Domain.Warehouses;
using Xunit;

namespace InventoryService.UnitTests.Stock;

public class GetStockByIdHandlerTests
{
    private readonly Mock<ILogger<global::InventoryService.Application.Stock.GetStockByIdHandler>> _loggerMock = new();
    private readonly Mock<global::InventoryService.Application.Stock.IStockRepository> _repositoryMock = new();

    private static global::InventoryService.Domain.Stock.Stock CreateStockWithNav(Product product, Warehouse warehouse, int qoh = 100)
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(product.Id, warehouse.Id) { QuantityOnHand = qoh, ReorderLevel = 10 };
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Product")!.SetValue(stock, product);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Warehouse")!.SetValue(stock, warehouse);
        return stock;
    }

    [Fact]
    public async Task Handle_WithExistingId_ShouldReturnStockDto()
    {
        var product = new Product("Test", "T-001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, 20);
        var warehouse = new Warehouse { Name = "WH", Code = "WH-1" };
        var stock = CreateStockWithNav(product, warehouse, qoh: 100);

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.GetStockByIdHandler(_loggerMock.Object, _repositoryMock.Object);
        var query = new global::InventoryService.Application.Stock.GetStockByIdQuery(stock.Id);
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.QuantityOnHand.Should().Be(100);
        result.Value!.AvailableQuantity.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::InventoryService.Domain.Stock.Stock?)null);

        var handler = new global::InventoryService.Application.Stock.GetStockByIdHandler(_loggerMock.Object, _repositoryMock.Object);
        var query = new global::InventoryService.Application.Stock.GetStockByIdQuery(Guid.NewGuid());
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STOCK_NOT_FOUND");
    }
}
