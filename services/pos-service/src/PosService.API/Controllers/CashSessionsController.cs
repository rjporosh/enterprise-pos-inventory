using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosService.Application.CashSessions.CloseSession;
using PosService.Application.CashSessions.Dtos;
using PosService.Application.CashSessions.OpenSession;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/cash-sessions")]
public class CashSessionsController(IMediator mediator, ILogger<CashSessionsController> logger) : ControllerBase
{
    [HttpPost("open")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Open([FromBody] OpenSessionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new OpenSessionCommand(request), ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to open cash session: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Opened cash session {SessionId}", result.Value);
        return Ok(result.Value);
    }

    [HttpPost("close")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Close([FromBody] CloseSessionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CloseSessionCommand(request), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "CASH_SESSION_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            logger.LogWarning("Failed to close cash session {SessionId}: {Error}", request.SessionId, result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Closed cash session {SessionId}", request.SessionId);
        return NoContent();
    }
}
