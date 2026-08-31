using MediatR;
using PosService.Application.Registers.Dtos;
using SharedKernel;

namespace PosService.Application.Registers.GetAllRegisters;

public class GetAllRegistersHandler(ICashRegisterRepository registerRepository)
    : IRequestHandler<GetAllRegistersQuery, Result<IReadOnlyList<RegisterDto>>>
{
    public async Task<Result<IReadOnlyList<RegisterDto>>> Handle(GetAllRegistersQuery query, CancellationToken ct)
    {
        var registers = await registerRepository.GetAllAsync(ct);

        var filtered = query.StoreId.HasValue
            ? registers.Where(r => r.StoreId == query.StoreId.Value)
            : registers;

        IReadOnlyList<RegisterDto> dtos = filtered
            .Select(r => new RegisterDto(r.Id, r.Name, r.Code, r.StoreId, r.IsActive))
            .ToList();

        return Result<IReadOnlyList<RegisterDto>>.Success(dtos);
    }
}
