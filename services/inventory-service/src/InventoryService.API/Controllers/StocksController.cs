using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/v1/stocks")]
public class StocksController(IMediator mediator, ILogger<StocksController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(global::InventoryService.Application.Stock.StockDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Create([FromBody] global::InventoryService.Application.Stock.CreateStockRequest request, CancellationToken ct)
    {
        var command = new global::InventoryService.Application.Stock.CreateStockCommand(request);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to create stock record: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Created stock record {StockId}", result.Value!.Id);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(global::InventoryService.Application.Stock.StockDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var query = new global::InventoryService.Application.Stock.GetStockByIdQuery(id);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Stock record {StockId} not found: {Error}", id, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(global::InventoryService.Application.Stock.PagedResult<global::InventoryService.Application.Stock.StockListItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] bool? lowStock = null,
        [FromQuery] bool? outOfStock = null,
        [FromQuery] string? sortBy = "productName",
        [FromQuery] bool sortDescending = false,
        CancellationToken ct = default)
    {
        var query = new global::InventoryService.Application.Stock.GetAllStocksQuery(pageNumber, pageSize, productId, warehouseId, lowStock, outOfStock, sortBy, sortDescending);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to get stocks: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(global::InventoryService.Application.Stock.StockDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] global::InventoryService.Application.Stock.UpdateStockRequest request, CancellationToken ct)
    {
        if (id != request.Id)
        {
            return Problem(
                title: "ID_MISMATCH",
                detail: "Route ID and request body ID do not match.",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        var command = new global::InventoryService.Application.Stock.UpdateStockCommand(request);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code is "STOCK_NOT_FOUND" or "STOCK_DELETED"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            logger.LogWarning("Failed to update stock {StockId}: {Error}", id, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var command = new global::InventoryService.Application.Stock.DeleteStockCommand(id);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code is "STOCK_NOT_FOUND" or "STOCK_ALREADY_DELETED"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            logger.LogWarning("Failed to delete stock {StockId}: {Error}", id, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return NoContent();
    }

    [HttpPost("in")]
    [ProducesResponseType(typeof(global::InventoryService.Application.Stock.StockMovementDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> StockIn([FromBody] global::InventoryService.Application.Stock.StockInCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code is "STOCK_NOT_FOUND" or "STOCK_DELETED"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            logger.LogWarning("Stock In failed: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpPost("out")]
    [ProducesResponseType(typeof(global::InventoryService.Application.Stock.StockMovementDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> StockOut([FromBody] global::InventoryService.Application.Stock.StockOutCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code is "STOCK_NOT_FOUND" or "STOCK_DELETED"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            logger.LogWarning("Stock Out failed: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpPost("adjustment")]
    [ProducesResponseType(typeof(global::InventoryService.Application.Stock.StockMovementDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Adjustment([FromBody] global::InventoryService.Application.Stock.StockAdjustmentCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code is "STOCK_NOT_FOUND" or "STOCK_DELETED"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            logger.LogWarning("Stock Adjustment failed: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(global::InventoryService.Application.Stock.StockMovementDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<IActionResult> Transfer([FromBody] global::InventoryService.Application.Stock.StockTransferCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code is "STOCK_NOT_FOUND" or "STOCK_DELETED"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            logger.LogWarning("Stock Transfer failed: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }
}
