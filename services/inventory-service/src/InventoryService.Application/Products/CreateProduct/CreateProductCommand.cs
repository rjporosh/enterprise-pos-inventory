using MediatR;
using SharedKernel;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.CreateProduct;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<Result<Guid>>;
