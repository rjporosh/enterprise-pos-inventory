using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosService.Application.Stores.CreateStore;
using PosService.Application.Stores.Dtos;
using PosService.Application.Stores.GetAllStores;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/stores")]
public class StoresController(IMediator mediator, ILogger<StoresController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateStoreRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateStoreCommand(request), ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to create store: {Error}", result.Error);
            return Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path);
        }

        logger.LogInformation("Created store {StoreId}", result.Value);
        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StoreDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllStoresQuery(), ct);
        return Ok(result.Value);
    }
}
