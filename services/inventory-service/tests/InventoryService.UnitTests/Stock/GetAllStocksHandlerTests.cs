using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using global::InventoryService.Domain.Products;
using global::InventoryService.Domain.Warehouses;
using global::InventoryService.Domain.Stock;
using Xunit;

namespace InventoryService.UnitTests.Stock;

public class GetAllStocksHandlerTests
{
    private readonly Mock<ILogger<global::InventoryService.Application.Stock.GetAllStocksHandler>> _loggerMock = new();
    private readonly Mock<global::InventoryService.Application.Stock.IStockRepository> _repositoryMock = new();

    private static global::InventoryService.Domain.Stock.Stock CreateStockWithNav(Product product, Warehouse warehouse, int qoh = 50, int reorderLevel = 10)
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(product.Id, warehouse.Id) { QuantityOnHand = qoh, ReorderLevel = reorderLevel };
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Product")!.SetValue(stock, product);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Warehouse")!.SetValue(stock, warehouse);
        return stock;
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllStocks()
    {
        var product = new Product("P1", "SKU-1", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, 20);
        var warehouse = new Warehouse { Name = "WH1", Code = "WH-1" };
        var stock = CreateStockWithNav(product, warehouse, qoh: 50, reorderLevel: 10);

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([stock]);

        var handler = new global::InventoryService.Application.Stock.GetAllStocksHandler(_loggerMock.Object, _repositoryMock.Object);
        var query = new global::InventoryService.Application.Stock.GetAllStocksQuery(PageNumber: 1, PageSize: 10);
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Count.Should().Be(1);
        result.Value!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithLowStockFilter_ShouldReturnLowStockOnly()
    {
        var product = new Product("P1", "SKU-1", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, 20);
        var warehouse = new Warehouse { Name = "WH1", Code = "WH-1" };
        var lowStock = CreateStockWithNav(product, warehouse, qoh: 5, reorderLevel: 10);

        _repositoryMock.Setup(r => r.GetLowStockAsync(It.IsAny<CancellationToken>())).ReturnsAsync([lowStock]);

        var handler = new global::InventoryService.Application.Stock.GetAllStocksHandler(_loggerMock.Object, _repositoryMock.Object);
        var query = new global::InventoryService.Application.Stock.GetAllStocksQuery(LowStock: true);
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Count.Should().Be(1);
        result.Value!.Items[0].IsLowStock.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        var product = new Product("P1", "SKU-1", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, 20);
        var warehouse = new Warehouse { Name = "WH1", Code = "WH-1" };
        var stocks = Enumerable.Range(1, 25).Select(i =>
            CreateStockWithNav(product, warehouse, qoh: i * 10, reorderLevel: 5)
        ).ToList();

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stocks);

        var handler = new global::InventoryService.Application.Stock.GetAllStocksHandler(_loggerMock.Object, _repositoryMock.Object);
        var query = new global::InventoryService.Application.Stock.GetAllStocksQuery(PageNumber: 2, PageSize: 10);
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Count.Should().Be(10);
        result.Value!.TotalCount.Should().Be(25);
    }
}
