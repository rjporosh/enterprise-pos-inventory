using PosService.Domain.Registers;

namespace PosService.Application.CashSessions.Dtos;

public record OpenSessionRequest(Guid RegisterId, Guid CashierId, decimal OpeningBalance);

public record CloseSessionRequest(Guid SessionId, decimal ClosingBalance, decimal ExpectedBalance, string? Notes);

public record CashSessionDto(
    Guid Id,
    Guid RegisterId,
    Guid CashierId,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal OpeningBalance,
    decimal? ClosingBalance,
    decimal? ExpectedBalance,
    decimal? Variance,
    CashSessionStatus Status,
    string? Notes);
