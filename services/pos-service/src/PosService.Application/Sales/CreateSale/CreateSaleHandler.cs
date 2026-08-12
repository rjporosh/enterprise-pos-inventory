using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Cashiers;
using PosService.Application.Customers;
using PosService.Application.Registers;
using PosService.Application.Sales.Repositories;
using PosService.Application.Stores;
using PosService.Domain.Sales;
using SharedKernel;

namespace PosService.Application.Sales.CreateSale;

public class CreateSaleHandler(
    ILogger<CreateSaleHandler> logger,
    ISaleRepository saleRepository,
    IStoreRepository storeRepository,
    ICashRegisterRepository registerRepository,
    ICashierRepository cashierRepository,
    ICashSessionRepository cashSessionRepository,
    ICustomerRepository customerRepository) : IRequestHandler<CreateSaleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSaleCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var store = await storeRepository.GetByIdAsync(request.StoreId, ct);
        if (store is null || !store.IsActive)
        {
            return Result<Guid>.Failure(new Error("STORE_NOT_FOUND", $"Store '{request.StoreId}' was not found or is inactive."));
        }

        if (!await registerRepository.ExistsActiveAsync(request.RegisterId, ct))
        {
            return Result<Guid>.Failure(new Error("REGISTER_NOT_FOUND", $"Register '{request.RegisterId}' was not found or is inactive."));
        }

        if (!await cashierRepository.ExistsActiveAsync(request.CashierId, ct))
        {
            return Result<Guid>.Failure(new Error("CASHIER_NOT_FOUND", $"Cashier '{request.CashierId}' was not found or is inactive."));
        }

        var session = await cashSessionRepository.GetByIdAsync(request.CashSessionId, ct);
        if (session is null || session.Status != Domain.Registers.CashSessionStatus.Open)
        {
            return Result<Guid>.Failure(new Error("CASH_SESSION_NOT_OPEN", $"Cash session '{request.CashSessionId}' was not found or is not open."));
        }

        if (session.RegisterId != request.RegisterId || session.CashierId != request.CashierId)
        {
            return Result<Guid>.Failure(new Error("CASH_SESSION_MISMATCH", "The cash session does not belong to the given register and cashier."));
        }

        if (request.CustomerId.HasValue && !await customerRepository.ExistsActiveAsync(request.CustomerId.Value, ct))
        {
            return Result<Guid>.Failure(new Error("CUSTOMER_NOT_FOUND", $"Customer '{request.CustomerId}' was not found or is inactive."));
        }

        var today = DateTime.UtcNow.Date;
        var sequence = await saleRepository.GetNextSaleSequenceAsync(request.StoreId, today, ct);
        var saleNumber = $"{store.Code}-{today:yyyyMMdd}-{sequence:D4}";

        var sale = new Sale(saleNumber, request.StoreId, request.RegisterId, request.CashierId, request.CashSessionId, request.CustomerId);

        saleRepository.Add(sale);
        await saleRepository.SaveChangesAsync(ct);

        logger.LogInformation("Opened sale {SaleId} ({SaleNumber}) on register {RegisterId}", sale.Id, sale.SaleNumber, request.RegisterId);

        return sale.Id;
    }
}
