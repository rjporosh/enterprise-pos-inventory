using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedWeb;
using PosService.Application.Stores.CreateStore;
using PosService.Application.Stores.Dtos;
using PosService.Application.Stores.GetAllStores;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/v1/stores")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiFailureResponse), StatusCodes.Status400BadRequest)]
public class StoresController(IMediator mediator, ILogger<StoresController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateStoreRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateStoreCommand(request), ct);
        if (result.IsSuccess)
            logger.LogInformation("Created store {StoreId}", result.Value);
        else
            logger.LogWarning("Failed to create store: {Error}", result.Error);
        return this.ToApiResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StoreDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllStoresQuery(), ct);
        return this.ToApiResult(result);
    }
}
