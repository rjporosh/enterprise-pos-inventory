using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Sales.Events;
using PosService.Application.Sales.Repositories;
using PosService.Domain.Sales;
using SharedKernel;

namespace PosService.Application.Sales.VoidSale;

public class VoidSaleHandler(
    ILogger<VoidSaleHandler> logger,
    ISaleRepository saleRepository,
    ISaleEventPublisher eventPublisher) : IRequestHandler<VoidSaleCommand, Result>
{
    public async Task<Result> Handle(VoidSaleCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var sale = await saleRepository.GetByIdAsync(request.SaleId, ct);
        if (sale is null)
        {
            return Result.Failure(new Error("SALE_NOT_FOUND", $"Sale '{request.SaleId}' was not found."));
        }

        if (sale.Status == SaleStatus.Voided)
        {
            return Result.Failure(new Error("SALE_ALREADY_VOIDED", $"Sale '{sale.SaleNumber}' is already voided."));
        }

        var wasCompleted = sale.Status == SaleStatus.Completed;

        sale.Void(request.Reason);

        saleRepository.Update(sale);
        await saleRepository.SaveChangesAsync(ct);

        logger.LogInformation("Voided sale {SaleId} ({SaleNumber}): {Reason}", sale.Id, sale.SaleNumber, request.Reason);

        // Only a previously-completed sale could have already triggered downstream stock deduction in
        // Inventory, so only completed→voided transitions need a compensating event.
        if (wasCompleted)
        {
            try
            {
                await eventPublisher.PublishSaleVoidedAsync(sale, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish SaleVoided event for sale {SaleId}; sale remains voided", sale.Id);
            }
        }

        return Result.Success();
    }
}
