using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Sales.Repositories;
using PosService.Domain.Sales;
using SharedKernel;

namespace PosService.Application.Sales.RemoveSaleItem;

public class RemoveSaleItemHandler(
    ILogger<RemoveSaleItemHandler> logger,
    ISaleRepository saleRepository) : IRequestHandler<RemoveSaleItemCommand, Result>
{
    public async Task<Result> Handle(RemoveSaleItemCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var sale = await saleRepository.GetByIdAsync(request.SaleId, ct);
        if (sale is null)
        {
            return Result.Failure(new Error("SALE_NOT_FOUND", $"Sale '{request.SaleId}' was not found."));
        }

        if (sale.Status != SaleStatus.Draft)
        {
            return Result.Failure(new Error("SALE_NOT_EDITABLE", $"Sale '{sale.SaleNumber}' is {sale.Status} and can no longer be modified."));
        }

        var item = sale.Items.FirstOrDefault(i => i.Id == request.SaleItemId);
        if (item is null)
        {
            return Result.Failure(new Error("SALE_ITEM_NOT_FOUND", $"Line item '{request.SaleItemId}' was not found on sale '{sale.SaleNumber}'."));
        }

        sale.Items.Remove(item);
        sale.RecalculateTotals();

        saleRepository.Update(sale);
        await saleRepository.SaveChangesAsync(ct);

        logger.LogInformation("Removed item {SaleItemId} from sale {SaleId}", request.SaleItemId, sale.Id);

        return Result.Success();
    }
}
