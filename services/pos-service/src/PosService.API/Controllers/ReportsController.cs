using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedWeb;
using PosService.Application.Reporting;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Produces("application/json")]
public class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("daily-sales")]
    [ProducesResponseType(typeof(DailySalesReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDailySales([FromQuery] Guid storeId, [FromQuery] DateOnly reportDate, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDailySalesReportQuery(storeId, reportDate), ct);
        return this.ToApiResult(result);
    }
}
