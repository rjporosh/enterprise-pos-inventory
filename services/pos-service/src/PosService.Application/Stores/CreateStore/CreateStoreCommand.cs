using MediatR;
using PosService.Application.Stores.Dtos;
using SharedKernel;

namespace PosService.Application.Stores.CreateStore;

public record CreateStoreCommand(CreateStoreRequest Request) : IRequest<Result<Guid>>;
