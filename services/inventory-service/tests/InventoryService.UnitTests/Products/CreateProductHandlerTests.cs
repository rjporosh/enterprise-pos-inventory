using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using InventoryService.Domain.Products;
using InventoryService.Application.Products.CreateProduct;
using InventoryService.Application.Products.Repositories;
using InventoryService.Application.Products.Dtos;
using Xunit;

namespace InventoryService.UnitTests.Products;

public class CreateProductHandlerTests
{
    private readonly Mock<ILogger<CreateProductHandler>> _loggerMock = new();
    private readonly Mock<IProductRepository> _repositoryMock = new();

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateProduct()
    {
        _repositoryMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.BarcodeExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateProductHandler(_loggerMock.Object, _repositoryMock.Object);
        var request = new CreateProductRequest(
            Name: "Laptop",
            Description: "Gaming laptop",
            Sku: "LAP-001",
            Barcode: "123456789",
            CategoryId: Guid.NewGuid(),
            BrandId: Guid.NewGuid(),
            UnitId: Guid.NewGuid(),
            SupplierId: null,
            CostPrice: 50000,
            SellingPrice: 60000,
            DiscountPercent: null,
            TaxPercent: 5,
            ReorderLevel: 10,
            MaxStockLevel: 100);

        var command = new CreateProductCommand(request);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateSku_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.SkuExistsAsync("LAP-001", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CreateProductHandler(_loggerMock.Object, _repositoryMock.Object);
        var request = new CreateProductRequest(
            Name: "New Laptop",
            Description: null,
            Sku: "LAP-001",
            Barcode: null,
            CategoryId: Guid.NewGuid(),
            BrandId: Guid.NewGuid(),
            UnitId: Guid.NewGuid(),
            SupplierId: null,
            CostPrice: 50000,
            SellingPrice: 60000,
            DiscountPercent: null,
            TaxPercent: 5,
            ReorderLevel: 10,
            MaxStockLevel: 100);

        var command = new CreateProductCommand(request);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("PRODUCT_SKU_EXISTS");
    }
}
