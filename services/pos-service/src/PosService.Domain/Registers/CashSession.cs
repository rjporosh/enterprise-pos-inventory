using PosService.Domain.Cashiers;
using PosService.Domain.Common;
using SharedKernel;

namespace PosService.Domain.Registers;

/// <summary>
/// Represents one open-to-close cash drawer session on a register, worked by a single cashier.
/// A sale can only be completed against an Open session (enforced in the Application layer).
/// </summary>
public class CashSession : BaseEntity
{
    public Guid RegisterId { get; set; }
    public Guid CashierId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? ExpectedBalance { get; set; }
    public decimal? Variance { get; set; }
    public CashSessionStatus Status { get; set; } = CashSessionStatus.Open;
    public string? Notes { get; set; }

    public CashRegister Register { get; set; } = null!;
    public Cashier Cashier { get; set; } = null!;

    public CashSession() { }

    public CashSession(Guid registerId, Guid cashierId, decimal openingBalance)
    {
        RegisterId = registerId;
        CashierId = cashierId;
        OpeningBalance = Guard.NotNegative(openingBalance, nameof(openingBalance));
        OpenedAt = DateTime.UtcNow;
        Status = CashSessionStatus.Open;
    }

    public void Close(decimal closingBalance, decimal expectedBalance, string? notes = null)
    {
        if (Status == CashSessionStatus.Closed)
        {
            throw new InvalidOperationException("Cash session is already closed.");
        }

        ClosingBalance = Guard.NotNegative(closingBalance, nameof(closingBalance));
        ExpectedBalance = expectedBalance;
        Variance = closingBalance - expectedBalance;
        Notes = notes;
        ClosedAt = DateTime.UtcNow;
        Status = CashSessionStatus.Closed;
    }
}
