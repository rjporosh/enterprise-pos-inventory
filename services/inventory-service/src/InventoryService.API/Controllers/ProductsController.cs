using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedWeb;
using InventoryService.Application.Products.Dtos;
using InventoryService.Application.Products.CreateProduct;
using InventoryService.Application.Products.GetProductById;
using InventoryService.Application.Products.GetAllProducts;
using InventoryService.Application.Products.UpdateProduct;
using InventoryService.Application.Products.DeleteProduct;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
public class ProductsController(IMediator mediator, ILogger<ProductsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateProductCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Created product {ProductId}", result.Value);
        else
            logger.LogWarning("Failed to create product: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Product {ProductId} not found: {Error}", id, result.Error);
        return this.ToApiResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "name",
        [FromQuery] bool sortDescending = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAllProductsQuery(pageNumber, pageSize, categoryId, brandId, isActive, searchTerm, sortBy, sortDescending), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to get products: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        if (id != request.Id)
            return this.ValidationEnvelope("id", "ID_MISMATCH", "Route ID and request body ID do not match.");

        var result = await mediator.Send(new UpdateProductCommand(request), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to update product {ProductId}: {Error}", id, result.Error);
        return this.ToApiResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteProductCommand(id), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to delete product {ProductId}: {Error}", id, result.Error);
        return this.ToApiResult(result);
    }
}
