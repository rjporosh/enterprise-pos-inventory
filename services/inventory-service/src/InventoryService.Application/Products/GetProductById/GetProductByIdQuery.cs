using MediatR;
using SharedKernel;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
