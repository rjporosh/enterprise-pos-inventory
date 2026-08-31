using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Stores;
using PosService.Domain.Registers;
using SharedKernel;

namespace PosService.Application.Registers.CreateRegister;

public class CreateRegisterHandler(
    ILogger<CreateRegisterHandler> logger,
    ICashRegisterRepository registerRepository,
    IStoreRepository storeRepository) : IRequestHandler<CreateRegisterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRegisterCommand command, CancellationToken ct)
    {
        var request = command.Request;

        if (!await storeRepository.ExistsActiveAsync(request.StoreId, ct))
        {
            return Result<Guid>.Failure(new Error("STORE_NOT_FOUND", $"Store '{request.StoreId}' was not found or is inactive."));
        }

        if (await registerRepository.CodeExistsAsync(request.Code, ct: ct))
        {
            return Result<Guid>.Failure(new Error("REGISTER_CODE_EXISTS", $"A register with code '{request.Code}' already exists."));
        }

        var register = new CashRegister(request.Name, request.Code, request.StoreId);

        registerRepository.Add(register);
        await registerRepository.SaveChangesAsync(ct);

        logger.LogInformation("Created register {RegisterId} ({Code}) for store {StoreId}", register.Id, register.Code, request.StoreId);

        return register.Id;
    }
}
