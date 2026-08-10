using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using global::InventoryService.Domain.Stock;
using global::InventoryService.Domain.Products;
using global::InventoryService.Domain.Warehouses;
using Xunit;

namespace InventoryService.UnitTests.Stock;

public class StockMovementHandlerTests
{
    private readonly Mock<global::InventoryService.Application.Stock.IStockRepository> _repositoryMock = new();

    private static global::InventoryService.Domain.Stock.Stock CreateTestStock(int quantityOnHand = 100, int reorderLevel = 10, int maxStockLevel = 200)
    {
        var product = new Product("P1", "SKU-1", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, 20);
        var warehouse = new Warehouse { Name = "WH1", Code = "WH-1" };
        var stock = new global::InventoryService.Domain.Stock.Stock(product.Id, warehouse.Id, reorderLevel, maxStockLevel) { QuantityOnHand = quantityOnHand };
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Product")!.SetValue(stock, product);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Warehouse")!.SetValue(stock, warehouse);
        return stock;
    }

    private static Mock<ILogger<T>> CreateLoggerMock<T>() where T : class => new();

    [Fact]
    public async Task StockIn_WithValidRequest_ShouldIncreaseStock()
    {
        var stock = CreateTestStock();

        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(stock.ProductId, stock.WarehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.GetByIdAsync(stock.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.StockInHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockInHandler>().Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.StockInCommand(stock.ProductId, stock.WarehouseId, 50, 10m, "Purchase", Guid.NewGuid(), "PO-001");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Quantity.Should().Be(50);
        result.Value!.BalanceAfter.Should().Be(150);
        result.Value!.MovementType.Should().Be(global::InventoryService.Domain.Stock.StockMovementType.StockIn);
    }

    [Fact]
    public async Task StockIn_WithNonExistentStock_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::InventoryService.Domain.Stock.Stock?)null);

        var handler = new global::InventoryService.Application.Stock.StockInHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockInHandler>().Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.StockInCommand(Guid.NewGuid(), Guid.NewGuid(), 50, null, null, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STOCK_NOT_FOUND");
    }

    [Fact]
    public async Task StockIn_ExceedingMaxStockLevel_ShouldReturnFailure()
    {
        var stock = CreateTestStock(quantityOnHand: 180, maxStockLevel: 200);

        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(stock.ProductId, stock.WarehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.StockInHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockInHandler>().Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.StockInCommand(stock.ProductId, stock.WarehouseId, 50, null, null, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("MAX_STOCK_LEVEL_EXCEEDED");
    }

    [Fact]
    public async Task StockOut_WithValidRequest_ShouldDecreaseStock()
    {
        var stock = CreateTestStock(quantityOnHand: 100);

        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(stock.ProductId, stock.WarehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.GetByIdAsync(stock.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.StockOutHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockOutHandler>().Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.StockOutCommand(stock.ProductId, stock.WarehouseId, 30, "Sale", Guid.NewGuid(), "Order-123");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Quantity.Should().Be(-30);
        result.Value!.BalanceAfter.Should().Be(70);
        result.Value!.MovementType.Should().Be(global::InventoryService.Domain.Stock.StockMovementType.StockOut);
    }

    [Fact]
    public async Task StockOut_WithInsufficientStock_ShouldReturnFailure()
    {
        var stock = CreateTestStock(quantityOnHand: 20);

        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(stock.ProductId, stock.WarehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.StockOutHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockOutHandler>().Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.StockOutCommand(stock.ProductId, stock.WarehouseId, 50, null, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INSUFFICIENT_STOCK");
    }

    [Fact]
    public async Task StockAdjustment_WithPositiveChange_ShouldIncreaseStock()
    {
        var stock = CreateTestStock(quantityOnHand: 100);

        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(stock.ProductId, stock.WarehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.GetByIdAsync(stock.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.StockAdjustmentHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockAdjustmentHandler>().Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.StockAdjustmentCommand(stock.ProductId, stock.WarehouseId, 10, "Found extra units");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.BalanceAfter.Should().Be(110);
    }

    [Fact]
    public async Task StockAdjustment_WithNegativeChange_CausingNegativeStock_ShouldReturnFailure()
    {
        var stock = CreateTestStock(quantityOnHand: 5);

        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(stock.ProductId, stock.WarehouseId, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.StockAdjustmentHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockAdjustmentHandler>().Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.StockAdjustmentCommand(stock.ProductId, stock.WarehouseId, -10, "Damage");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INSUFFICIENT_STOCK");
    }

    [Fact]
    public async Task StockTransfer_WithValidRequest_ShouldTransferStock()
    {
        var product = new Product("P1", "SKU-1", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, 20);
        var fromWarehouse = new Warehouse { Name = "WH1", Code = "WH-1" };
        var toWarehouse = new Warehouse { Name = "WH2", Code = "WH-2" };

        var fromStock = new global::InventoryService.Domain.Stock.Stock(product.Id, fromWarehouse.Id) { QuantityOnHand = 100 };
        var toStock = new global::InventoryService.Domain.Stock.Stock(product.Id, toWarehouse.Id) { QuantityOnHand = 0 };

        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Product")!.SetValue(fromStock, product);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Warehouse")!.SetValue(fromStock, fromWarehouse);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Product")!.SetValue(toStock, product);
        typeof(global::InventoryService.Domain.Stock.Stock).GetProperty("Warehouse")!.SetValue(toStock, toWarehouse);

        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(fromStock.ProductId, fromWarehouse.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fromStock);
        _repositoryMock.Setup(r => r.GetByProductAndWarehouseAsync(toStock.ProductId, toWarehouse.Id, It.IsAny<CancellationToken>())).ReturnsAsync(toStock);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.GetByIdAsync(fromStock.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fromStock);

        var handler = new global::InventoryService.Application.Stock.StockTransferHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockTransferHandler>().Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.StockTransferCommand(product.Id, fromWarehouse.Id, toWarehouse.Id, 30, "Restock request");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MovementType.Should().Be(global::InventoryService.Domain.Stock.StockMovementType.TransferOut);
        result.Value!.Quantity.Should().Be(-30);
    }

    [Fact]
    public async Task StockTransfer_WithSameWarehouse_ShouldReturnFailure()
    {
        var handler = new global::InventoryService.Application.Stock.StockTransferHandler(CreateLoggerMock<global::InventoryService.Application.Stock.StockTransferHandler>().Object, _repositoryMock.Object);
        var warehouseId = Guid.NewGuid();
        var command = new global::InventoryService.Application.Stock.StockTransferCommand(Guid.NewGuid(), warehouseId, warehouseId, 10, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
