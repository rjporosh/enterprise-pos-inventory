using MediatR;
using PosService.Application.Sales.Dtos;
using PosService.Domain.Sales;
using SharedKernel;

namespace PosService.Application.Sales.GetAllSales;

public record GetAllSalesQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? StoreId = null,
    Guid? CashierId = null,
    SaleStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<Result<PagedResult<SaleListItemDto>>>;
