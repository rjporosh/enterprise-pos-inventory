using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
public class SalesController(IMediator mediator, ILogger<SalesController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateSaleCommand(request), ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to open sale: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Opened sale {SaleId}", result.Value);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSaleByIdQuery(id), ct);

        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SaleListItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
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

        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> AddItem([FromBody] AddSaleItemRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddSaleItemCommand(request), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "SALE_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            logger.LogWarning("Failed to add sale item: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }

    [HttpDelete("items")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> RemoveItem([FromBody] RemoveSaleItemRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveSaleItemCommand(request), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code is "SALE_NOT_FOUND" or "SALE_ITEM_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            logger.LogWarning("Failed to remove sale item: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return NoContent();
    }

    [HttpPost("complete")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Complete([FromBody] CompleteSaleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CompleteSaleCommand(request), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "SALE_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            logger.LogWarning("Failed to complete sale {SaleId}: {Error}", request.SaleId, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Completed sale {SaleId}", request.SaleId);
        return NoContent();
    }

    [HttpPost("void")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Void([FromBody] VoidSaleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new VoidSaleCommand(request), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "SALE_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            logger.LogWarning("Failed to void sale {SaleId}: {Error}", request.SaleId, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Voided sale {SaleId}", request.SaleId);
        return NoContent();
    }
}
