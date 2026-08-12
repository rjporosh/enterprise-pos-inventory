using MediatR;
using PosService.Application.Sales.Dtos;
using SharedKernel;

namespace PosService.Application.Sales.VoidSale;

public record VoidSaleCommand(VoidSaleRequest Request) : IRequest<Result>;
