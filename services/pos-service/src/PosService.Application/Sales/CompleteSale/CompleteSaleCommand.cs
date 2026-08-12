using MediatR;
using PosService.Application.Sales.Dtos;
using SharedKernel;

namespace PosService.Application.Sales.CompleteSale;

public record CompleteSaleCommand(CompleteSaleRequest Request) : IRequest<Result>;
