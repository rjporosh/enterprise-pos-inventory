using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.Application.Stock;

public class DeleteStockHandler(
    ILogger<DeleteStockHandler> logger,
    IStockRepository repository) : IRequestHandler<DeleteStockCommand, Result>
{
    public async Task<Result> Handle(DeleteStockCommand command, CancellationToken ct)
    {
        var stock = await repository.GetByIdAsync(command.Id, ct);

        if (stock is null)
        {
            return Result.Failure(new Error("STOCK_NOT_FOUND",
                $"Stock record with ID '{command.Id}' was not found."));
        }

        if (stock.IsDeleted)
        {
            return Result.Failure(new Error("STOCK_ALREADY_DELETED",
                $"Stock record with ID '{command.Id}' has already been deleted."));
        }

        repository.SoftDelete(stock);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Soft-deleted stock record {StockId}.", command.Id);

        return Result.Success();
    }
}
