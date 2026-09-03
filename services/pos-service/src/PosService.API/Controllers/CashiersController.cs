using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedWeb;
using PosService.Application.Cashiers.Dtos;
using PosService.Application.Cashiers.EnsureCashier;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/cashiers")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
public class CashiersController(IMediator mediator, ILogger<CashiersController> logger) : ControllerBase
{
    /// <summary>
    /// Get-or-create by Username (idempotent) — bridges an auth-service user (identified by
    /// email) to a pos-service Cashier record. See EnsureCashierRequest's doc comment.
    /// </summary>
    [HttpPost("ensure")]
    [ProducesResponseType(typeof(CashierDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Ensure([FromBody] EnsureCashierRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new EnsureCashierCommand(request), ct);
        if (!result.IsSuccess)
            logger.LogWarning("Failed to ensure cashier: {Error}", result.Error);
        return this.ToApiResult(result);
    }
}
