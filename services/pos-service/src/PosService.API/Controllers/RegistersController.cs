using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedWeb;
using PosService.Application.Registers.CreateRegister;
using PosService.Application.Registers.Dtos;
using PosService.Application.Registers.GetAllRegisters;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/registers")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status404NotFound)]
public class RegistersController(IMediator mediator, ILogger<RegistersController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateRegisterRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateRegisterCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Created register {RegisterId}", result.Value);
        else
            logger.LogWarning("Failed to create register: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RegisterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllRegistersQuery(storeId), ct);
        return this.ToApiResult(result);
    }
}
