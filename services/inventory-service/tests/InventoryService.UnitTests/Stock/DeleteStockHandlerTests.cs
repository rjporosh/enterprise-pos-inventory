using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using global::InventoryService.Domain.Stock;
using Xunit;

namespace InventoryService.UnitTests.Stock;

public class DeleteStockHandlerTests
{
    private readonly Mock<ILogger<global::InventoryService.Application.Stock.DeleteStockHandler>> _loggerMock = new();
    private readonly Mock<global::InventoryService.Application.Stock.IStockRepository> _repositoryMock = new();

    [Fact]
    public async Task Handle_WithExistingId_ShouldSoftDelete()
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(Guid.NewGuid(), Guid.NewGuid()) { IsDeleted = false };
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.SoftDelete(It.IsAny<global::InventoryService.Domain.Stock.Stock>())).Callback<global::InventoryService.Domain.Stock.Stock>(s =>
        {
            s.IsDeleted = true;
            s.DeletedAt = DateTime.UtcNow;
        });

        var handler = new global::InventoryService.Application.Stock.DeleteStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.DeleteStockCommand(stock.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stock.IsDeleted.Should().BeTrue();
        stock.DeletedAt.Should().NotBeNull();
        _repositoryMock.Verify(r => r.SoftDelete(stock), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::InventoryService.Domain.Stock.Stock?)null);

        var handler = new global::InventoryService.Application.Stock.DeleteStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.DeleteStockCommand(Guid.NewGuid());
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STOCK_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithAlreadyDeletedId_ShouldReturnFailure()
    {
        var stock = new global::InventoryService.Domain.Stock.Stock(Guid.NewGuid(), Guid.NewGuid()) { IsDeleted = true };
        _repositoryMock.Setup(r => r.GetByIdAsync(stock.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var handler = new global::InventoryService.Application.Stock.DeleteStockHandler(_loggerMock.Object, _repositoryMock.Object);
        var command = new global::InventoryService.Application.Stock.DeleteStockCommand(stock.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STOCK_ALREADY_DELETED");
        _repositoryMock.Verify(r => r.SoftDelete(It.IsAny<global::InventoryService.Domain.Stock.Stock>()), Times.Never);
    }
}
