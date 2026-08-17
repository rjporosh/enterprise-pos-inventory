using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record CreateStockCommand(CreateStockRequest Request) : IRequest<Result<StockDto>>;
