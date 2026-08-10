using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedKernel;
using InventoryService.Application.Products.Dtos;
using InventoryService.Application.Products.CreateProduct;
using InventoryService.Application.Products.GetProductById;
using InventoryService.Application.Products.GetAllProducts;
using InventoryService.Application.Products.UpdateProduct;
using InventoryService.Application.Products.DeleteProduct;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController(IMediator mediator, ILogger<ProductsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand(request);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to create product: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Created product {ProductId}", result.Value);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Product {ProductId} not found: {Error}", id, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductListItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
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
        var query = new GetAllProductsQuery(pageNumber, pageSize, categoryId, brandId, isActive, searchTerm, sortBy, sortDescending);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to get products: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        if (id != request.Id)
        {
            return Problem(
                title: "ID_MISMATCH",
                detail: "Route ID and request body ID do not match.",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        var command = new UpdateProductCommand(request);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "PRODUCT_NOT_FOUND" || result.Error.Code == "PRODUCT_DELETED"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            logger.LogWarning("Failed to update product {ProductId}: {Error}", id, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var command = new DeleteProductCommand(id);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "PRODUCT_NOT_FOUND" || result.Error.Code == "PRODUCT_ALREADY_DELETED"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            logger.LogWarning("Failed to delete product {ProductId}: {Error}", id, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return NoContent();
    }
}
