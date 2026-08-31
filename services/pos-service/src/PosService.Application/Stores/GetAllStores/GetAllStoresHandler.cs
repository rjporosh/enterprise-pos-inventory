using MediatR;
using PosService.Application.Stores.Dtos;
using SharedKernel;

namespace PosService.Application.Stores.GetAllStores;

public class GetAllStoresHandler(IStoreRepository storeRepository)
    : IRequestHandler<GetAllStoresQuery, Result<IReadOnlyList<StoreDto>>>
{
    public async Task<Result<IReadOnlyList<StoreDto>>> Handle(GetAllStoresQuery query, CancellationToken ct)
    {
        var stores = await storeRepository.GetAllAsync(ct);

        IReadOnlyList<StoreDto> dtos = stores
            .Select(s => new StoreDto(s.Id, s.Name, s.Code, s.Address, s.City, s.Country, s.Phone, s.Email, s.Currency, s.IsActive))
            .ToList();

        return Result<IReadOnlyList<StoreDto>>.Success(dtos);
    }
}
