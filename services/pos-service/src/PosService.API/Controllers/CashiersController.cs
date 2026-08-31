using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosService.Application.Cashiers.Dtos;
using PosService.Application.Cashiers.EnsureCashier;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/cashiers")]
public class CashiersController(IMediator mediator, ILogger<CashiersController> logger) : ControllerBase
{
    /// <summary>
    /// Get-or-create by Username (idempotent) — bridges an auth-service user (identified by
    /// email) to a pos-service Cashier record. See EnsureCashierRequest's doc comment.
    /// </summary>
    [HttpPost("ensure")]
    [ProducesResponseType(typeof(CashierDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Ensure([FromBody] EnsureCashierRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new EnsureCashierCommand(request), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "STORE_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            logger.LogWarning("Failed to ensure cashier: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Value);
    }
}
