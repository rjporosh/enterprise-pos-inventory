using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record DeleteStockCommand(Guid Id) : IRequest<Result>;
