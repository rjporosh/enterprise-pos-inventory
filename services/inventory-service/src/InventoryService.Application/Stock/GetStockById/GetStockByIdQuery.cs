using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record GetStockByIdQuery(Guid Id) : IRequest<Result<StockDto>>;
