using MediatR;
using PosService.Application.CashSessions.Dtos;
using SharedKernel;

namespace PosService.Application.CashSessions.CloseSession;

public record CloseSessionCommand(CloseSessionRequest Request) : IRequest<Result>;
