using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Domain.Stores;
using SharedKernel;

namespace PosService.Application.Stores.CreateStore;

public class CreateStoreHandler(
    ILogger<CreateStoreHandler> logger,
    IStoreRepository storeRepository) : IRequestHandler<CreateStoreCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStoreCommand command, CancellationToken ct)
    {
        var request = command.Request;

        if (await storeRepository.CodeExistsAsync(request.Code, ct: ct))
        {
            return Result<Guid>.Failure(new Error("STORE_CODE_EXISTS", $"A store with code '{request.Code}' already exists."));
        }

        var store = new Store(request.Name, request.Code, request.Currency)
        {
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            Phone = request.Phone,
            Email = request.Email,
        };

        storeRepository.Add(store);
        await storeRepository.SaveChangesAsync(ct);

        logger.LogInformation("Created store {StoreId} ({Code})", store.Id, store.Code);

        return store.Id;
    }
}
