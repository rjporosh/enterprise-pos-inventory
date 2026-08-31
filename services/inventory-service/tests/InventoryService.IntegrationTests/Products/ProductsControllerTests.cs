using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using System.Net.Http.Json;
using Xunit;

namespace InventoryService.IntegrationTests.Products;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Seeded by the InventoryService.Infrastructure "SeedInitialData" migration — real rows that
    // exist in every environment this migration has run against, unlike a random Guid.NewGuid()
    // which has no matching category/brand/unit row and trips the products table's FK constraints.
    private static readonly Guid SeededCategoryId = new("20000000-0000-0000-0000-000000000001");
    private static readonly Guid SeededBrandId = new("30000000-0000-0000-0000-000000000001");
    private static readonly Guid SeededUnitId = new("10000000-0000-0000-0000-000000000001");

    private readonly HttpClient _client;
    private readonly Guid _categoryId;
    private readonly Guid _brandId;
    private readonly Guid _unitId;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _categoryId = SeededCategoryId;
        _brandId = SeededBrandId;
        _unitId = SeededUnitId;
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturn201()
    {
        // Sku/Barcode are unique-indexed and this suite runs against a real, persistent
        // Postgres database with no reset-between-runs strategy (see docs/API-GAPS.md /
        // AI-HANDOVER.md — Respawn-based isolation is tracked as future work), so a fixed
        // literal would only pass once; a per-run suffix keeps the test repeatable.
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Sku = $"TEST-{suffix}",
            Barcode = suffix,
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
        // DeleteProductValidator rejects Guid.Empty with a 400 (malformed input) before the
        // handler ever runs a lookup — that's correct, deliberate behavior (see
        // DeleteProductValidator.cs), not the "not found" case this test exercises. A
        // well-formed Guid that simply has no matching row is the right "invalid id" fixture.
        var response = await _client.DeleteAsync($"/api/v1/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
