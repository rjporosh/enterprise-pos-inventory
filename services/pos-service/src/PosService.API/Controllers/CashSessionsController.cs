using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedWeb;
using PosService.Application.CashSessions.CloseSession;
using PosService.Application.CashSessions.Dtos;
using PosService.Application.CashSessions.OpenSession;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/cash-sessions")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
public class CashSessionsController(IMediator mediator, ILogger<CashSessionsController> logger) : ControllerBase
{
    [HttpPost("open")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Open([FromBody] OpenSessionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new OpenSessionCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Opened cash session {SessionId}", result.Value);
        else
            logger.LogWarning("Failed to open cash session: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpPost("close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Close([FromBody] CloseSessionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CloseSessionCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Closed cash session {SessionId}", request.SessionId);
        else
            logger.LogWarning("Failed to close cash session {SessionId}: {Error}", request.SessionId, result.Error);
        return this.ToApiResult(result);
    }
}
