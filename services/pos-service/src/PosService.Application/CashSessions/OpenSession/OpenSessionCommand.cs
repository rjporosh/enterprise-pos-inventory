using MediatR;
using PosService.Application.CashSessions.Dtos;
using SharedKernel;

namespace PosService.Application.CashSessions.OpenSession;

public record OpenSessionCommand(OpenSessionRequest Request) : IRequest<Result<Guid>>;
