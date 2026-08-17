using FluentValidation;

namespace PosService.Application.Sales.CompleteSale;

public class CompleteSaleValidator : AbstractValidator<CompleteSaleCommand>
{
    public CompleteSaleValidator()
    {
        RuleFor(x => x.Request.SaleId).NotEmpty().WithMessage("Sale is required.");
        RuleFor(x => x.Request.Payments).NotEmpty().WithMessage("At least one payment is required.");
        RuleForEach(x => x.Request.Payments).ChildRules(payment =>
        {
            payment.RuleFor(p => p.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        });
    }
}
