using MediatR;
using SharedKernel;

namespace InventoryService.Application.Products.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<Result>;
