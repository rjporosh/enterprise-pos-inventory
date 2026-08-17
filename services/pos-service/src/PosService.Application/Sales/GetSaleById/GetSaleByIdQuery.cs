using MediatR;
using PosService.Application.Sales.Dtos;
using SharedKernel;

namespace PosService.Application.Sales.GetSaleById;

public record GetSaleByIdQuery(Guid Id) : IRequest<Result<SaleDto>>;
