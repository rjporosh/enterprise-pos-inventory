using FluentValidation;

namespace PosService.Application.CashSessions.CloseSession;

public class CloseSessionValidator : AbstractValidator<CloseSessionCommand>
{
    public CloseSessionValidator()
    {
        RuleFor(x => x.Request.SessionId).NotEmpty().WithMessage("Session is required.");
        RuleFor(x => x.Request.ClosingBalance).GreaterThanOrEqualTo(0).WithMessage("Closing balance cannot be negative.");
        RuleFor(x => x.Request.ExpectedBalance).GreaterThanOrEqualTo(0).WithMessage("Expected balance cannot be negative.");
        RuleFor(x => x.Request.Notes).MaximumLength(1000);
    }
}
