using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedWeb;
using Stock = InventoryService.Application.Stock;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/v1/stocks")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
public class StocksController(IMediator mediator, ILogger<StocksController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Stock.StockDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] Stock.CreateStockRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new Stock.CreateStockCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Created stock record {StockId}", result.Value!.Id);
        else
            logger.LogWarning("Failed to create stock record: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Stock.StockDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new Stock.GetStockByIdQuery(id), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Stock record {StockId} not found: {Error}", id, result.Error);
        return this.ToApiResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Stock.PagedResult<Stock.StockListItemDto>), StatusCodes.Status200OK)]
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
        var result = await mediator.Send(
            new Stock.GetAllStocksQuery(pageNumber, pageSize, productId, warehouseId, lowStock, outOfStock, sortBy, sortDescending), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to get stocks: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Stock.StockDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Stock.UpdateStockRequest request, CancellationToken ct)
    {
        if (id != request.Id)
            return this.ValidationEnvelope("id", "ID_MISMATCH", "Route ID and request body ID do not match.");

        var result = await mediator.Send(new Stock.UpdateStockCommand(request), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to update stock {StockId}: {Error}", id, result.Error);
        return this.ToApiResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new Stock.DeleteStockCommand(id), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to delete stock {StockId}: {Error}", id, result.Error);
        return this.ToApiResult(result);
    }

    [HttpPost("in")]
    [ProducesResponseType(typeof(Stock.StockMovementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> StockIn([FromBody] Stock.StockInCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
            logger.LogWarning("Stock In failed: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpPost("out")]
    [ProducesResponseType(typeof(Stock.StockMovementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> StockOut([FromBody] Stock.StockOutCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
            logger.LogWarning("Stock Out failed: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpPost("adjustment")]
    [ProducesResponseType(typeof(Stock.StockMovementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Adjustment([FromBody] Stock.StockAdjustmentCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
            logger.LogWarning("Stock Adjustment failed: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(Stock.StockMovementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Transfer([FromBody] Stock.StockTransferCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
            logger.LogWarning("Stock Transfer failed: {Error}", result.Error);
        return this.ToApiResult(result);
    }
}
