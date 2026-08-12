using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Sales.Dtos;
using PosService.Application.Sales.Repositories;
using SharedKernel;

namespace PosService.Application.Sales.GetSaleById;

public class GetSaleByIdHandler(
    ILogger<GetSaleByIdHandler> logger,
    ISaleRepository saleRepository) : IRequestHandler<GetSaleByIdQuery, Result<SaleDto>>
{
    public async Task<Result<SaleDto>> Handle(GetSaleByIdQuery query, CancellationToken ct)
    {
        var sale = await saleRepository.GetByIdAsync(query.Id, ct);

        if (sale is null)
        {
            return Result<SaleDto>.Failure(new Error("SALE_NOT_FOUND", $"Sale '{query.Id}' was not found."));
        }

        logger.LogInformation("Retrieved sale {SaleId}", sale.Id);

        var dto = new SaleDto(
            sale.Id,
            sale.SaleNumber,
            sale.StoreId,
            sale.RegisterId,
            sale.CashierId,
            sale.CashSessionId,
            sale.CustomerId,
            sale.SaleDate,
            sale.Status,
            sale.SubtotalAmount,
            sale.DiscountAmount,
            sale.TaxAmount,
            sale.TotalAmount,
            sale.PaidAmount,
            sale.ChangeAmount,
            sale.VoidReason,
            sale.Items.Select(i => new SaleItemDto(i.Id, i.ProductId, i.ProductName, i.Sku, i.UnitPrice, i.Quantity, i.DiscountAmount, i.TaxAmount, i.LineTotal)).ToList(),
            sale.Payments.Select(p => new PaymentDto(p.Id, p.Method, p.Amount, p.ReferenceNumber, p.PaidAt)).ToList());

        return dto;
    }
}
