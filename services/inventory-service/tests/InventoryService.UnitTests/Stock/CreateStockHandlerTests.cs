using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using global::InventoryService.Domain.Stock;
using global::InventoryService.Domain.Products;
using global::InventoryService.Domain.Warehouses;
using Xunit;

namespace InventoryService.UnitTests.Stock;

public class CreateStockHandlerTests
{
    private readonly Mock<ILogger<global::InventoryService.Application.Stock.CreateStockHandler>> _loggerMock = new();
    private readonly Mock<global::InventoryService.Application.Stock.IStockRepository> _repositoryMock = new();
    private readonly Dictionary<Guid, global::InventoryService.Domain.Stock.Stock> _stockStore = new();
    private Product? _product;
    private Warehouse? _warehouse;

    private static global::InventoryService.Domain.Stock.Stock CreateStockWithNav(Product product, Warehouse warehouse, int qoh = 0)
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(product.Id, warehouse.Id) { QuantityOnHand = qoh };
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Product")!.SetValue(stock, product);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Warehouse")!.SetValue(stock, warehouse);
        return stock;
    }

    private void SetupRepositoryMock()
    {
        _repositoryMock.Setup(r => r.ExistsForProductWarehouseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.Add(It.IsAny<global::InventoryService.Domain.Stock.Stock>())).Callback<global::InventoryService.Domain.Stock.Stock>(s => _stockStore[s.Id] = s);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns<Guid, CancellationToken>((id, _) =>
        {
            if (_stockStore.TryGetValue(id, out var s))
            {
                if (s.Product == null && _product != null)
                    typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Product")!.SetValue(s, _product);
                if (s.Warehouse == null && _warehouse != null)
                    typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Warehouse")!.SetValue(s, _warehouse);
                return Task.FromResult<global::InventoryService.Domain.Stock.Stock?>(s);
            }
            return Task.FromResult<global::InventoryService.Domain.Stock.Stock?>(null);
        });
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateStock()
    {
        _product = new Product("Test Product", "TEST-001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100, 200);
        _warehouse = new Warehouse { Name = "Main WH", Code = "WH-001" };
        SetupRepositoryMock();

        var handler = new global::InventoryService.Application.Stock.CreateStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var request = new global::InventoryService.Application.Stock.CreateStockRequest(
            ProductId: _product.Id,
            WarehouseId: _warehouse.Id,
            InitialQuantity: 50,
            ReorderLevel: 10,
            MaxStockLevel: 200,
            UnitCost: 100);

        var command = new global::InventoryService.Application.Stock.CreateStockCommand(request);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.QuantityOnHand.Should().Be(50);
        _repositoryMock.Verify(r => r.Add(It.IsAny<global::InventoryService.Domain.Stock.Stock>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStockAlreadyExists_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.ExistsForProductWarehouseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new global::InventoryService.Application.Stock.CreateStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var request = new global::InventoryService.Application.Stock.CreateStockRequest(
            ProductId: Guid.NewGuid(),
            WarehouseId: Guid.NewGuid(),
            InitialQuantity: 50,
            ReorderLevel: 10,
            MaxStockLevel: 200,
            null);

        var command = new global::InventoryService.Application.Stock.CreateStockCommand(request);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STOCK_ALREADY_EXISTS");
        _repositoryMock.Verify(r => r.Add(It.IsAny<global::InventoryService.Domain.Stock.Stock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithZeroInitialQuantity_ShouldCreateStockWithZeroBalance()
    {
        _product = new Product("Test Product", "TEST-001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100, 200);
        _warehouse = new Warehouse { Name = "Main WH", Code = "WH-001" };
        SetupRepositoryMock();

        var handler = new global::InventoryService.Application.Stock.CreateStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var request = new global::InventoryService.Application.Stock.CreateStockRequest(
            ProductId: _product.Id,
            WarehouseId: _warehouse.Id,
            InitialQuantity: 0,
            ReorderLevel: 5,
            MaxStockLevel: 100,
            null);

        var command = new global::InventoryService.Application.Stock.CreateStockCommand(request);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.QuantityOnHand.Should().Be(0);
    }
}
