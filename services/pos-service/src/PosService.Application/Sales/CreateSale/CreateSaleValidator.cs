using FluentValidation;

namespace PosService.Application.Sales.CreateSale;

public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.Request.StoreId).NotEmpty().WithMessage("Store is required.");
        RuleFor(x => x.Request.RegisterId).NotEmpty().WithMessage("Register is required.");
        RuleFor(x => x.Request.CashierId).NotEmpty().WithMessage("Cashier is required.");
        RuleFor(x => x.Request.CashSessionId).NotEmpty().WithMessage("Cash session is required.");
    }
}
