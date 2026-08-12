using MediatR;
using PosService.Application.Sales.Dtos;
using SharedKernel;

namespace PosService.Application.Sales.AddSaleItem;

public record AddSaleItemCommand(AddSaleItemRequest Request) : IRequest<Result<Guid>>;
