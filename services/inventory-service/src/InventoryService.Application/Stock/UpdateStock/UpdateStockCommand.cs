using MediatR;
using SharedKernel;

namespace InventoryService.Application.Stock;

public record UpdateStockCommand(UpdateStockRequest Request) : IRequest<Result<StockDto>>;
