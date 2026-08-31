using MediatR;
using PosService.Application.Registers.Dtos;
using SharedKernel;

namespace PosService.Application.Registers.GetAllRegisters;

public record GetAllRegistersQuery(Guid? StoreId) : IRequest<Result<IReadOnlyList<RegisterDto>>>;
