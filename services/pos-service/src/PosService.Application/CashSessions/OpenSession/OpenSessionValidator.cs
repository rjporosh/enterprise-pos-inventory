using FluentValidation;

namespace PosService.Application.CashSessions.OpenSession;

public class OpenSessionValidator : AbstractValidator<OpenSessionCommand>
{
    public OpenSessionValidator()
    {
        RuleFor(x => x.Request.RegisterId).NotEmpty().WithMessage("Register is required.");
        RuleFor(x => x.Request.CashierId).NotEmpty().WithMessage("Cashier is required.");
        RuleFor(x => x.Request.OpeningBalance).GreaterThanOrEqualTo(0).WithMessage("Opening balance cannot be negative.");
    }
}
