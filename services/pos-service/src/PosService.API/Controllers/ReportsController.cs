using MediatR;
using Microsoft.AspNetCore.Mvc;
using PosService.Application.Reporting;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("daily-sales")]
    [ProducesResponseType(typeof(DailySalesReportDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetDailySales([FromQuery] Guid storeId, [FromQuery] DateOnly reportDate, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDailySalesReportQuery(storeId, reportDate), ct);

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
}
