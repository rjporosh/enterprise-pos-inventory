using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PosService.Application.Cashiers;
using PosService.Application.Customers;
using PosService.Application.Registers;
using PosService.Application.Sales.CreateSale;
using PosService.Application.Sales.Dtos;
using PosService.Application.Sales.Repositories;
using PosService.Application.Stores;
using PosService.Domain.Registers;
using PosService.Domain.Stores;
using Xunit;

namespace PosService.UnitTests.Sales;

public class CreateSaleHandlerTests
{
    private readonly Mock<ILogger<CreateSaleHandler>> _loggerMock = new();
    private readonly Mock<ISaleRepository> _saleRepoMock = new();
    private readonly Mock<IStoreRepository> _storeRepoMock = new();
    private readonly Mock<ICashRegisterRepository> _registerRepoMock = new();
    private readonly Mock<ICashierRepository> _cashierRepoMock = new();
    private readonly Mock<ICashSessionRepository> _sessionRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();

    private CreateSaleHandler CreateHandler() => new(
        _loggerMock.Object,
        _saleRepoMock.Object,
        _storeRepoMock.Object,
        _registerRepoMock.Object,
        _cashierRepoMock.Object,
        _sessionRepoMock.Object,
        _customerRepoMock.Object);

    private void SetupHappyPath(Guid storeId, Guid registerId, Guid cashierId, Guid sessionId)
    {
        var store = new Store("Test Store", "TST");
        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _registerRepoMock.Setup(r => r.ExistsActiveAsync(registerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _cashierRepoMock.Setup(r => r.ExistsActiveAsync(cashierId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var session = new CashSession(registerId, cashierId, 500m);
        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _saleRepoMock.Setup(r => r.GetNextSaleSequenceAsync(storeId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _saleRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateSale()
    {
        var storeId = Guid.NewGuid();
        var registerId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        SetupHappyPath(storeId, registerId, cashierId, sessionId);

        var request = new CreateSaleRequest(storeId, registerId, cashierId, sessionId, null);
        var result = await CreateHandler().Handle(new CreateSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _saleRepoMock.Verify(r => r.Add(It.IsAny<PosService.Domain.Sales.Sale>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveStore_ShouldReturnFailure()
    {
        var storeId = Guid.NewGuid();
        var store = new Store("Test Store", "TST");
        store.Deactivate();
        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);

        var request = new CreateSaleRequest(storeId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var result = await CreateHandler().Handle(new CreateSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STORE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithNullStore_ShouldReturnFailure()
    {
        var storeId = Guid.NewGuid();
        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync((Store?)null);

        var request = new CreateSaleRequest(storeId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var result = await CreateHandler().Handle(new CreateSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("STORE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithInactiveRegister_ShouldReturnFailure()
    {
        var storeId = Guid.NewGuid();
        var registerId = Guid.NewGuid();
        var store = new Store("Test Store", "TST");
        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _registerRepoMock.Setup(r => r.ExistsActiveAsync(registerId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var request = new CreateSaleRequest(storeId, registerId, Guid.NewGuid(), Guid.NewGuid(), null);
        var result = await CreateHandler().Handle(new CreateSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("REGISTER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithInactiveCashier_ShouldReturnFailure()
    {
        var storeId = Guid.NewGuid();
        var registerId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();
        var store = new Store("Test Store", "TST");
        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _registerRepoMock.Setup(r => r.ExistsActiveAsync(registerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _cashierRepoMock.Setup(r => r.ExistsActiveAsync(cashierId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var request = new CreateSaleRequest(storeId, registerId, cashierId, Guid.NewGuid(), null);
        var result = await CreateHandler().Handle(new CreateSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("CASHIER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithNullCashSession_ShouldReturnFailure()
    {
        var storeId = Guid.NewGuid();
        var registerId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var store = new Store("Test Store", "TST");
        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _registerRepoMock.Setup(r => r.ExistsActiveAsync(registerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _cashierRepoMock.Setup(r => r.ExistsActiveAsync(cashierId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync((CashSession?)null);

        var request = new CreateSaleRequest(storeId, registerId, cashierId, sessionId, null);
        var result = await CreateHandler().Handle(new CreateSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("CASH_SESSION_NOT_OPEN");
    }
}
