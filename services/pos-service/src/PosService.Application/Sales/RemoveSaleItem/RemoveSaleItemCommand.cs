using MediatR;
using PosService.Application.Sales.Dtos;
using SharedKernel;

namespace PosService.Application.Sales.RemoveSaleItem;

public record RemoveSaleItemCommand(RemoveSaleItemRequest Request) : IRequest<Result>;
