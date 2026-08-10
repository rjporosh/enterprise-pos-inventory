using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using global::InventoryService.Domain.Stock;
using global::InventoryService.Domain.Products;
using global::InventoryService.Domain.Warehouses;
using Xunit;

namespace InventoryService.UnitTests.Stock;

public class UpdateStockHandlerTests
{
    private readonly Mock<ILogger<global::InventoryService.Application.Stock.UpdateStockHandler>> _loggerMock = new();
    private readonly Mock<global::InventoryService.Application.Stock.IStockRepository> _repositoryMock = new();

    private static global::InventoryService.Domain.Stock.Stock CreateStockWithNav(Product product, Warehouse warehouse, int reorderLevel = 5, int maxStockLevel = 100)
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(product.Id, warehouse.Id, reorderLevel, maxStockLevel);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Product")!.SetValue(stock, product);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Warehouse")!.SetValue(stock, warehouse);
        return stock;
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateStock()
    {
        var product = new Product("Test", "T-001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, 20);
        var warehouse = new Warehouse { Name = "WH", Code = "WH-1" };
        var stock = CreateStockWithNav(product, warehouse, reorderLevel: 5, maxStockLevel: 100);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::InventoryService.Domain.Stock.Stock?)null);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new global::InventoryService.Application.Stock.UpdateStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var newProductId = Guid.NewGuid();
        var newWarehouseId = Guid.NewGuid();
        var request = new global::InventoryService.Application.Stock.UpdateStockRequest(stock.Id, newProductId, newWarehouseId, 20, 500);

        var command = new global::InventoryService.Application.Stock.UpdateStockCommand(request);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stock.ReorderLevel.Should().Be(20);
        stock.MaxStockLevel.Should().Be(500);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::InventoryService.Domain.Stock.Stock?)null);

        var handler = new global::InventoryService.Application.Stock.UpdateStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var request = new global::InventoryService.Application.Stock.UpdateStockRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, 100);
        var command = new global::InventoryService.Application.Stock.UpdateStockCommand(request);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STOCK_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithDeletedStock_ShouldReturnFailure()
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(Guid.NewGuid(), Guid.NewGuid()) { IsDeleted = true };
        _repositoryMock.Setup(r => r.GetByIdAsync(stock.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.UpdateStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var request = new global::InventoryService.Application.Stock.UpdateStockRequest(stock.Id, Guid.NewGuid(), Guid.NewGuid(), 10, 100);
        var command = new global::InventoryService.Application.Stock.UpdateStockCommand(request);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STOCK_DELETED");
    }
}
