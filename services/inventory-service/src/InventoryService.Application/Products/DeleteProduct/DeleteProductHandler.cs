using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;
using InventoryService.Application.Products.Repositories;

namespace InventoryService.Application.Products.DeleteProduct;

public class DeleteProductHandler(
    ILogger<DeleteProductHandler> logger,
    IProductRepository repository) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(command.Id, ct);

        if (product is null)
        {
            return Result.Failure(new Error("PRODUCT_NOT_FOUND", $"Product with ID '{command.Id}' was not found."));
        }

        if (product.IsDeleted)
        {
            return Result.Failure(new Error("PRODUCT_ALREADY_DELETED", $"Product with ID '{command.Id}' has already been deleted."));
        }

        repository.SoftDelete(product);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Soft-deleted product {ProductId}", product.Id);

        return Result.Success();
    }
}
