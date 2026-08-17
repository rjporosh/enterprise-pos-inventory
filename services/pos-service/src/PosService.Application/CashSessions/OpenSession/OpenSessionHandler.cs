using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Cashiers;
using PosService.Application.Registers;
using PosService.Domain.Registers;
using SharedKernel;

namespace PosService.Application.CashSessions.OpenSession;

public class OpenSessionHandler(
    ILogger<OpenSessionHandler> logger,
    ICashSessionRepository sessionRepository,
    ICashRegisterRepository registerRepository,
    ICashierRepository cashierRepository) : IRequestHandler<OpenSessionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(OpenSessionCommand command, CancellationToken ct)
    {
        var request = command.Request;

        if (!await registerRepository.ExistsActiveAsync(request.RegisterId, ct))
        {
            return Result<Guid>.Failure(new Error("REGISTER_NOT_FOUND", $"Register '{request.RegisterId}' was not found or is inactive."));
        }

        if (!await cashierRepository.ExistsActiveAsync(request.CashierId, ct))
        {
            return Result<Guid>.Failure(new Error("CASHIER_NOT_FOUND", $"Cashier '{request.CashierId}' was not found or is inactive."));
        }

        if (await sessionRepository.HasOpenSessionAsync(request.RegisterId, ct))
        {
            return Result<Guid>.Failure(new Error("REGISTER_SESSION_ALREADY_OPEN", $"Register '{request.RegisterId}' already has an open cash session."));
        }

        var session = new CashSession(request.RegisterId, request.CashierId, request.OpeningBalance);

        sessionRepository.Add(session);
        await sessionRepository.SaveChangesAsync(ct);

        logger.LogInformation("Opened cash session {SessionId} on register {RegisterId} for cashier {CashierId}", session.Id, request.RegisterId, request.CashierId);

        return session.Id;
    }
}
