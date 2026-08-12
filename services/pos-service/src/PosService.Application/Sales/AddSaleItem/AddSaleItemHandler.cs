using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Sales.Repositories;
using PosService.Domain.Sales;
using SharedKernel;

namespace PosService.Application.Sales.AddSaleItem;

public class AddSaleItemHandler(
    ILogger<AddSaleItemHandler> logger,
    ISaleRepository saleRepository) : IRequestHandler<AddSaleItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddSaleItemCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var sale = await saleRepository.GetByIdAsync(request.SaleId, ct);
        if (sale is null)
        {
            return Result<Guid>.Failure(new Error("SALE_NOT_FOUND", $"Sale '{request.SaleId}' was not found."));
        }

        if (sale.Status != SaleStatus.Draft)
        {
            return Result<Guid>.Failure(new Error("SALE_NOT_EDITABLE", $"Sale '{sale.SaleNumber}' is {sale.Status} and can no longer be modified."));
        }

        var existingItem = sale.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem is not null)
        {
            existingItem.ChangeQuantity(existingItem.Quantity + request.Quantity);
        }
        else
        {
            var item = new SaleItem(sale.Id, request.ProductId, request.ProductName, request.Sku, request.UnitPrice, request.Quantity);
            if (request.DiscountAmount > 0) item.ApplyDiscount(request.DiscountAmount);
            if (request.TaxAmount > 0) item.ApplyTax(request.TaxAmount);
            sale.Items.Add(item);
        }

        sale.RecalculateTotals();

        saleRepository.Update(sale);
        await saleRepository.SaveChangesAsync(ct);

        logger.LogInformation("Added product {ProductId} x{Quantity} to sale {SaleId}", request.ProductId, request.Quantity, sale.Id);

        return sale.Id;
    }
}
