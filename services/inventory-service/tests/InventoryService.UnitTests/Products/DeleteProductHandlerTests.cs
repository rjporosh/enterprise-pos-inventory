using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using InventoryService.Application.Products.DeleteProduct;
using InventoryService.Application.Products.Repositories;
using InventoryService.Domain.Products;
using Xunit;

namespace InventoryService.UnitTests.Products;

public class DeleteProductHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingProduct_ShouldSoftDelete()
    {
        var product = new Product("ToDelete", "DEL-001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100, 200);

        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(product);
        repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        repositoryMock.Setup(r => r.SoftDelete(It.IsAny<Product>()))
            .Callback<Product>(p =>
            {
                p.IsDeleted = true;
                p.DeletedAt = DateTime.UtcNow;
            });

        var loggerMock = new Mock<ILogger<DeleteProductHandler>>();
        var handler = new DeleteProductHandler(loggerMock.Object, repositoryMock.Object);
        var command = new DeleteProductCommand(product.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.IsDeleted.Should().BeTrue();
        product.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistingProduct_ShouldReturnFailure()
    {
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var loggerMock = new Mock<ILogger<DeleteProductHandler>>();
        var handler = new DeleteProductHandler(loggerMock.Object, repositoryMock.Object);
        var command = new DeleteProductCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("PRODUCT_NOT_FOUND");
    }
}
