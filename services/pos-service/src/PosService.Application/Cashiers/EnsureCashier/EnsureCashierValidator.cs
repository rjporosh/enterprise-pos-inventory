using FluentValidation;

namespace PosService.Application.Cashiers.EnsureCashier;

public class EnsureCashierValidator : AbstractValidator<EnsureCashierCommand>
{
    public EnsureCashierValidator()
    {
        RuleFor(x => x.Request.StoreId).NotEmpty().WithMessage("Store is required.");
        RuleFor(x => x.Request.Username).NotEmpty().MaximumLength(100).WithMessage("Username is required.");
        RuleFor(x => x.Request.FullName).NotEmpty().MaximumLength(200).WithMessage("Full name is required.");
    }
}
