using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using System.Net.Http.Json;
using Xunit;

namespace InventoryService.IntegrationTests.Products;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<object>>
{
    private readonly HttpClient _client;
    private readonly Guid _categoryId;
    private readonly Guid _brandId;
    private readonly Guid _unitId;

    public ProductsControllerTests(WebApplicationFactory<object> factory)
    {
        _client = factory.CreateClient();
        _categoryId = Guid.NewGuid();
        _brandId = Guid.NewGuid();
        _unitId = Guid.NewGuid();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturn201()
    {
        var request = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Sku = "TEST-001",
            Barcode = "123456",
            CategoryId = _categoryId,
            BrandId = _brandId,
            UnitId = _unitId,
            CostPrice = 1000,
            SellingPrice = 1500,
            DiscountPercent = 10,
            TaxPercent = 5,
            ReorderLevel = 10,
            MaxStockLevel = 100
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var productId = await response.Content.ReadFromJsonAsync<Guid>();
        productId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/v1/products/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/v1/products?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ShouldReturn404()
    {
        var request = new
        {
            Id = Guid.NewGuid(),
            Name = "Updated",
            Sku = "UPD-001",
            CategoryId = _categoryId,
            BrandId = _brandId,
            UnitId = _unitId,
            CostPrice = 1000,
            SellingPrice = 1500,
            IsActive = true,
            TrackInventory = true
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/products/{request.Id}", request);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ShouldReturn404()
    {
        var response = await _client.DeleteAsync("/api/v1/products/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
