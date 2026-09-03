using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedWeb;
using PosService.Application.Sales.AddSaleItem;
using PosService.Application.Sales.CompleteSale;
using PosService.Application.Sales.CreateSale;
using PosService.Application.Sales.Dtos;
using PosService.Application.Sales.GetAllSales;
using PosService.Application.Sales.GetSaleById;
using PosService.Application.Sales.RemoveSaleItem;
using PosService.Application.Sales.VoidSale;
using PosService.Domain.Sales;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
public class SalesController(IMediator mediator, ILogger<SalesController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateSaleCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Opened sale {SaleId}", result.Value);
        else
            logger.LogWarning("Failed to open sale: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSaleByIdQuery(id), ct);
        return this.ToApiResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SaleListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? storeId = null,
        [FromQuery] Guid? cashierId = null,
        [FromQuery] SaleStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAllSalesQuery(pageNumber, pageSize, storeId, cashierId, status, fromDate, toDate), ct);
        return this.ToApiResult(result);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddItem([FromBody] AddSaleItemRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddSaleItemCommand(request), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to add sale item: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpDelete("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItem([FromBody] RemoveSaleItemRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveSaleItemCommand(request), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to remove sale item: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Complete([FromBody] CompleteSaleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CompleteSaleCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Completed sale {SaleId}", request.SaleId);
        else
            logger.LogWarning("Failed to complete sale {SaleId}: {Error}", request.SaleId, result.Error);
        return this.ToApiResult(result);
    }

    [HttpPost("void")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Void([FromBody] VoidSaleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new VoidSaleCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Voided sale {SaleId}", request.SaleId);
        else
            logger.LogWarning("Failed to void sale {SaleId}: {Error}", request.SaleId, result.Error);
        return this.ToApiResult(result);
    }
}
