using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Cashiers.Dtos;
using PosService.Application.Stores;
using PosService.Domain.Cashiers;
using SharedKernel;

namespace PosService.Application.Cashiers.EnsureCashier;

public class EnsureCashierHandler(
    ILogger<EnsureCashierHandler> logger,
    ICashierRepository cashierRepository,
    IStoreRepository storeRepository) : IRequestHandler<EnsureCashierCommand, Result<CashierDto>>
{
    public async Task<Result<CashierDto>> Handle(EnsureCashierCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var existing = await cashierRepository.GetByUsernameAsync(request.Username, ct);
        if (existing is not null)
        {
            // Already registered as a cashier (possibly at a different store from a prior
            // session) — returned as-is rather than silently reassigning their home store.
            return ToDto(existing);
        }

        if (!await storeRepository.ExistsActiveAsync(request.StoreId, ct))
        {
            return Result<CashierDto>.Failure(new Error("STORE_NOT_FOUND", $"Store '{request.StoreId}' was not found or is inactive."));
        }

        var cashier = new Cashier(request.FullName, request.Username, request.StoreId)
        {
            Email = request.Email,
            Phone = request.Phone,
        };

        cashierRepository.Add(cashier);
        await cashierRepository.SaveChangesAsync(ct);

        logger.LogInformation("Created cashier {CashierId} ({Username}) for store {StoreId}", cashier.Id, cashier.Username, request.StoreId);

        return ToDto(cashier);
    }

    private static CashierDto ToDto(Cashier c) => new(c.Id, c.FullName, c.Username, c.Email, c.Phone, c.StoreId, c.IsActive);
}
