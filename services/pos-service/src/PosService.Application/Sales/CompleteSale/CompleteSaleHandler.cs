using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Sales.Events;
using PosService.Application.Sales.Repositories;
using PosService.Domain.Sales;
using SharedKernel;

namespace PosService.Application.Sales.CompleteSale;

public class CompleteSaleHandler(
    ILogger<CompleteSaleHandler> logger,
    ISaleRepository saleRepository,
    ISaleEventPublisher eventPublisher) : IRequestHandler<CompleteSaleCommand, Result>
{
    public async Task<Result> Handle(CompleteSaleCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var sale = await saleRepository.GetByIdAsync(request.SaleId, ct);
        if (sale is null)
        {
            return Result.Failure(new Error("SALE_NOT_FOUND", $"Sale '{request.SaleId}' was not found."));
        }

        if (sale.Status != SaleStatus.Draft)
        {
            return Result.Failure(new Error("SALE_NOT_EDITABLE", $"Sale '{sale.SaleNumber}' is {sale.Status} and cannot be completed."));
        }

        if (sale.Items.Count == 0)
        {
            return Result.Failure(new Error("SALE_EMPTY", $"Sale '{sale.SaleNumber}' has no line items."));
        }

        if (request.Payments.Count == 0)
        {
            return Result.Failure(new Error("PAYMENT_REQUIRED", "At least one payment is required to complete a sale."));
        }

        sale.RecalculateTotals();

        var totalPaid = request.Payments.Sum(p => p.Amount);
        if (totalPaid < sale.TotalAmount)
        {
            return Result.Failure(new Error("INSUFFICIENT_PAYMENT", $"Total payment {totalPaid:0.00} is less than the sale total {sale.TotalAmount:0.00}."));
        }

        foreach (var paymentRequest in request.Payments)
        {
            sale.Payments.Add(new Payment(sale.Id, paymentRequest.Method, paymentRequest.Amount, paymentRequest.ReferenceNumber));
        }

        sale.Complete(totalPaid);

        saleRepository.Update(sale);
        await saleRepository.SaveChangesAsync(ct);

        logger.LogInformation("Completed sale {SaleId} ({SaleNumber}) for {Total:0.00}", sale.Id, sale.SaleNumber, sale.TotalAmount);

        // Fire-and-record: publishing failures must never roll back or fail an already-committed sale.
        // The publisher is responsible for its own retry/outbox strategy (see Infrastructure implementation).
        try
        {
            await eventPublisher.PublishSaleCompletedAsync(sale, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish SaleCompleted event for sale {SaleId}; sale remains completed", sale.Id);
        }

        return Result.Success();
    }
}
