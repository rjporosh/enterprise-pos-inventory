using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using InventoryService.Application.Products.GetAllProducts;
using InventoryService.Application.Products.Repositories;
using InventoryService.Domain.Products;
using InventoryService.Domain.Catalog;
using Xunit;

namespace InventoryService.UnitTests.Products;

public class GetAllProductsHandlerTests
{
    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllProducts()
    {
        var category = new Category { Name = "Electronics" };
        var brand = new Brand { Name = "TechBrand" };
        var unit = new Unit { Name = "Pieces", Symbol = "pcs" };

        var products = new List<Product>
        {
            new Product
            {
                Name = "Product A",
                Sku = "A-001",
                CategoryId = category.Id,
                BrandId = brand.Id,
                UnitId = unit.Id,
                CostPrice = 100,
                SellingPrice = 200,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Category = category,
                Brand = brand,
                Unit = unit
            },
            new Product
            {
                Name = "Product B",
                Sku = "B-001",
                CategoryId = category.Id,
                BrandId = brand.Id,
                UnitId = unit.Id,
                CostPrice = 200,
                SellingPrice = 300,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Category = category,
                Brand = brand,
                Unit = unit
            }
        };

        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, "name", false, It.IsAny<CancellationToken>())).ReturnsAsync(products);
        repositoryMock.Setup(r => r.GetTotalCountAsync(null, null, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var loggerMock = new Mock<ILogger<GetAllProductsHandler>>();
        var handler = new GetAllProductsHandler(loggerMock.Object, repositoryMock.Object);
        var query = new GetAllProductsQuery(PageNumber: 1, PageSize: 10);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Count.Should().Be(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);
    }
}
