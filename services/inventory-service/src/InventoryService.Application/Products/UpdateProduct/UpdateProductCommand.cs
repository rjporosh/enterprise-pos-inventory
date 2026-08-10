using MediatR;
using SharedKernel;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.UpdateProduct;

public record UpdateProductCommand(UpdateProductRequest Request) : IRequest<Result>;
