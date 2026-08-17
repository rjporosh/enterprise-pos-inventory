using MediatR;
using Microsoft.Extensions.Logging;
using PosService.Application.Registers;
using SharedKernel;

namespace PosService.Application.CashSessions.CloseSession;

public class CloseSessionHandler(
    ILogger<CloseSessionHandler> logger,
    ICashSessionRepository sessionRepository) : IRequestHandler<CloseSessionCommand, Result>
{
    public async Task<Result> Handle(CloseSessionCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var session = await sessionRepository.GetByIdAsync(request.SessionId, ct);
        if (session is null)
        {
            return Result.Failure(new Error("CASH_SESSION_NOT_FOUND", $"Cash session '{request.SessionId}' was not found."));
        }

        try
        {
            session.Close(request.ClosingBalance, request.ExpectedBalance, request.Notes);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("CASH_SESSION_ALREADY_CLOSED", ex.Message));
        }

        sessionRepository.Update(session);
        await sessionRepository.SaveChangesAsync(ct);

        logger.LogInformation("Closed cash session {SessionId} with variance {Variance:0.00}", session.Id, session.Variance);

        return Result.Success();
    }
}
