using MediatR;
using PosService.Application.Sales.Dtos;
using SharedKernel;

namespace PosService.Application.Sales.CreateSale;

public record CreateSaleCommand(CreateSaleRequest Request) : IRequest<Result<Guid>>;
