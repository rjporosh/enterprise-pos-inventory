using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosService.Application.Registers.CreateRegister;
using PosService.Application.Registers.Dtos;
using PosService.Application.Registers.GetAllRegisters;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/registers")]
public class RegistersController(IMediator mediator, ILogger<RegistersController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateRegisterRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateRegisterCommand(request), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "STORE_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            logger.LogWarning("Failed to create register: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: statusCode,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Created register {RegisterId}", result.Value);
        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RegisterDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllRegistersQuery(storeId), ct);
        return Ok(result.Value);
    }
}
