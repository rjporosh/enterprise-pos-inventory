using MediatR;
using PosService.Application.Stores.Dtos;
using SharedKernel;

namespace PosService.Application.Stores.GetAllStores;

public record GetAllStoresQuery : IRequest<Result<IReadOnlyList<StoreDto>>>;
