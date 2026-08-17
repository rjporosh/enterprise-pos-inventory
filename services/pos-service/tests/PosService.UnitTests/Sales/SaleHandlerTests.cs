using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PosService.Application.Sales.AddSaleItem;
using PosService.Application.Sales.CompleteSale;
using PosService.Application.Sales.Dtos;
using PosService.Application.Sales.Events;
using PosService.Application.Sales.GetSaleById;
using PosService.Application.Sales.Repositories;
using PosService.Application.Sales.VoidSale;
using PosService.Domain.Sales;
using Xunit;

namespace PosService.UnitTests.Sales;

public class SaleHandlerTests
{
    // ── AddSaleItemHandler ──────────────────────────────────────────────────

    [Fact]
    public async Task AddSaleItem_WhenSaleNotFound_ShouldReturnFailure()
    {
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Sale?)null);
        var logger = new Mock<ILogger<AddSaleItemHandler>>();
        var handler = new AddSaleItemHandler(logger.Object, repoMock.Object);

        var request = new AddSaleItemRequest(Guid.NewGuid(), Guid.NewGuid(), "Product A", "SKU-001", 100m, 2);
        var result = await handler.Handle(new AddSaleItemCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("SALE_NOT_FOUND");
    }

    [Fact]
    public async Task AddSaleItem_WhenSaleIsCompleted_ShouldReturnFailure()
    {
        var sale = MakeCompletedSale();
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        var handler = new AddSaleItemHandler(new Mock<ILogger<AddSaleItemHandler>>().Object, repoMock.Object);

        var request = new AddSaleItemRequest(sale.Id, Guid.NewGuid(), "Product A", "SKU-001", 100m, 1);
        var result = await handler.Handle(new AddSaleItemCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("SALE_NOT_EDITABLE");
    }

    [Fact]
    public async Task AddSaleItem_WithValidRequest_ShouldAddItemAndReturnSaleId()
    {
        var sale = MakeDraftSale();
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = new AddSaleItemHandler(new Mock<ILogger<AddSaleItemHandler>>().Object, repoMock.Object);

        var request = new AddSaleItemRequest(sale.Id, Guid.NewGuid(), "Widget", "WID-001", 49.99m, 3);
        var result = await handler.Handle(new AddSaleItemCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sale.Items.Should().HaveCount(1);
        sale.Items.First().Quantity.Should().Be(3);
    }

    [Fact]
    public async Task AddSaleItem_WithDuplicateProduct_ShouldAggregateQuantity()
    {
        var sale = MakeDraftSale();
        var productId = Guid.NewGuid();
        sale.Items.Add(new SaleItem(sale.Id, productId, "Widget", "WID-001", 49.99m, 2));

        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = new AddSaleItemHandler(new Mock<ILogger<AddSaleItemHandler>>().Object, repoMock.Object);

        var request = new AddSaleItemRequest(sale.Id, productId, "Widget", "WID-001", 49.99m, 3);
        var result = await handler.Handle(new AddSaleItemCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sale.Items.Should().HaveCount(1);
        sale.Items.First().Quantity.Should().Be(5); // 2 + 3
    }

    // ── CompleteSaleHandler ─────────────────────────────────────────────────

    [Fact]
    public async Task CompleteSale_WhenSaleNotFound_ShouldReturnFailure()
    {
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Sale?)null);
        var handler = new CompleteSaleHandler(
            new Mock<ILogger<CompleteSaleHandler>>().Object,
            repoMock.Object,
            new Mock<ISaleEventPublisher>().Object);

        var request = new CompleteSaleRequest(Guid.NewGuid(), new[] { new SalePaymentRequest(PaymentMethodType.Cash, 100m, null) });
        var result = await handler.Handle(new CompleteSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("SALE_NOT_FOUND");
    }

    [Fact]
    public async Task CompleteSale_WithNoItems_ShouldReturnFailure()
    {
        var sale = MakeDraftSale();
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        var handler = new CompleteSaleHandler(
            new Mock<ILogger<CompleteSaleHandler>>().Object,
            repoMock.Object,
            new Mock<ISaleEventPublisher>().Object);

        var request = new CompleteSaleRequest(sale.Id, new[] { new SalePaymentRequest(PaymentMethodType.Cash, 100m, null) });
        var result = await handler.Handle(new CompleteSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("SALE_EMPTY");
    }

    [Fact]
    public async Task CompleteSale_WithNoPayments_ShouldReturnFailure()
    {
        var sale = MakeDraftSaleWithItem();
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        var handler = new CompleteSaleHandler(
            new Mock<ILogger<CompleteSaleHandler>>().Object,
            repoMock.Object,
            new Mock<ISaleEventPublisher>().Object);

        var request = new CompleteSaleRequest(sale.Id, Array.Empty<SalePaymentRequest>());
        var result = await handler.Handle(new CompleteSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("PAYMENT_REQUIRED");
    }

    [Fact]
    public async Task CompleteSale_WithInsufficientPayment_ShouldReturnFailure()
    {
        var sale = MakeDraftSaleWithItem(unitPrice: 100m, quantity: 2); // total = 200
        sale.RecalculateTotals();
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        var handler = new CompleteSaleHandler(
            new Mock<ILogger<CompleteSaleHandler>>().Object,
            repoMock.Object,
            new Mock<ISaleEventPublisher>().Object);

        var request = new CompleteSaleRequest(sale.Id, new[] { new SalePaymentRequest(PaymentMethodType.Cash, 50m, null) });
        var result = await handler.Handle(new CompleteSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INSUFFICIENT_PAYMENT");
    }

    [Fact]
    public async Task CompleteSale_WithValidPayment_ShouldSucceedAndPublishEvent()
    {
        var sale = MakeDraftSaleWithItem(unitPrice: 100m, quantity: 1); // total = 100
        sale.RecalculateTotals();
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var publisherMock = new Mock<ISaleEventPublisher>();
        var handler = new CompleteSaleHandler(
            new Mock<ILogger<CompleteSaleHandler>>().Object,
            repoMock.Object,
            publisherMock.Object);

        var request = new CompleteSaleRequest(sale.Id, new[] { new SalePaymentRequest(PaymentMethodType.Cash, 120m, null) });
        var result = await handler.Handle(new CompleteSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Completed);
        sale.PaidAmount.Should().Be(120m);
        sale.ChangeAmount.Should().Be(20m);
        publisherMock.Verify(p => p.PublishSaleCompletedAsync(sale, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── VoidSaleHandler ─────────────────────────────────────────────────────

    [Fact]
    public async Task VoidSale_WhenSaleNotFound_ShouldReturnFailure()
    {
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Sale?)null);
        var handler = new VoidSaleHandler(
            new Mock<ILogger<VoidSaleHandler>>().Object,
            repoMock.Object,
            new Mock<ISaleEventPublisher>().Object);

        var request = new VoidSaleRequest(Guid.NewGuid(), "Test reason");
        var result = await handler.Handle(new VoidSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("SALE_NOT_FOUND");
    }

    [Fact]
    public async Task VoidSale_WhenAlreadyVoided_ShouldReturnFailure()
    {
        var sale = MakeDraftSale();
        sale.Void("already voided");
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        var handler = new VoidSaleHandler(
            new Mock<ILogger<VoidSaleHandler>>().Object,
            repoMock.Object,
            new Mock<ISaleEventPublisher>().Object);

        var request = new VoidSaleRequest(sale.Id, "void again");
        var result = await handler.Handle(new VoidSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("SALE_ALREADY_VOIDED");
    }

    [Fact]
    public async Task VoidSale_WithDraftSale_ShouldSucceedWithoutPublishingEvent()
    {
        var sale = MakeDraftSale();
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var publisherMock = new Mock<ISaleEventPublisher>();
        var handler = new VoidSaleHandler(
            new Mock<ILogger<VoidSaleHandler>>().Object,
            repoMock.Object,
            publisherMock.Object);

        var request = new VoidSaleRequest(sale.Id, "customer walked out");
        var result = await handler.Handle(new VoidSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Voided);
        // Draft → Voided should NOT publish — no stock was deducted
        publisherMock.Verify(p => p.PublishSaleVoidedAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VoidSale_WithCompletedSale_ShouldPublishVoidedEvent()
    {
        var sale = MakeDraftSaleWithItem(unitPrice: 50m, quantity: 1);
        sale.RecalculateTotals();
        sale.Payments.Add(new Payment(sale.Id, PaymentMethodType.Cash, 50m, null));
        sale.Complete(50m);

        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var publisherMock = new Mock<ISaleEventPublisher>();
        var handler = new VoidSaleHandler(
            new Mock<ILogger<VoidSaleHandler>>().Object,
            repoMock.Object,
            publisherMock.Object);

        var request = new VoidSaleRequest(sale.Id, "returned goods");
        var result = await handler.Handle(new VoidSaleCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        publisherMock.Verify(p => p.PublishSaleVoidedAsync(sale, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetSaleByIdHandler ──────────────────────────────────────────────────

    [Fact]
    public async Task GetSaleById_WhenNotFound_ShouldReturnFailure()
    {
        var repoMock = new Mock<ISaleRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Sale?)null);
        var handler = new GetSaleByIdHandler(new Mock<ILogger<GetSaleByIdHandler>>().Object, repoMock.Object);

        var result = await handler.Handle(new GetSaleByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("SALE_NOT_FOUND");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static Sale MakeDraftSale()
    {
        var sale = new Sale("TST-20260816-0001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        return sale;
    }

    private static Sale MakeDraftSaleWithItem(decimal unitPrice = 100m, int quantity = 1)
    {
        var sale = MakeDraftSale();
        sale.Items.Add(new SaleItem(sale.Id, Guid.NewGuid(), "Widget", "WID-001", unitPrice, quantity));
        return sale;
    }

    private static Sale MakeCompletedSale()
    {
        var sale = MakeDraftSaleWithItem();
        sale.RecalculateTotals();
        sale.Payments.Add(new Payment(sale.Id, PaymentMethodType.Cash, 100m, null));
        sale.Complete(100m);
        return sale;
    }
}
