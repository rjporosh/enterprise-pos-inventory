using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Sales.Dtos;
using PosService.Application.Sales.Repositories;
using SharedKernel;

namespace PosService.Application.Sales.GetAllSales;

public class GetAllSalesHandler(
    ILogger<GetAllSalesHandler> logger,
    ISaleRepository saleRepository) : IRequestHandler<GetAllSalesQuery, Result<PagedResult<SaleListItemDto>>>
{
    public async Task<Result<PagedResult<SaleListItemDto>>> Handle(GetAllSalesQuery query, CancellationToken ct)
    {
        var sales = await saleRepository.GetPagedAsync(
            query.PageNumber, query.PageSize, query.StoreId, query.CashierId, query.Status, query.FromDate, query.ToDate, ct);

        var totalCount = await saleRepository.GetTotalCountAsync(
            query.StoreId, query.CashierId, query.Status, query.FromDate, query.ToDate, ct);

        logger.LogInformation("Retrieved {Count} sales (total: {Total})", sales.Count, totalCount);

        var items = sales.Select(s => new SaleListItemDto(s.Id, s.SaleNumber, s.SaleDate, s.Status, s.TotalAmount, s.CashierId, s.StoreId)).ToList();

        return new PagedResult<SaleListItemDto>(items, totalCount, query.PageNumber, query.PageSize);
    }
}
