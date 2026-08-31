using MediatR;
using PosService.Application.Registers.Dtos;
using SharedKernel;

namespace PosService.Application.Registers.CreateRegister;

public record CreateRegisterCommand(CreateRegisterRequest Request) : IRequest<Result<Guid>>;
