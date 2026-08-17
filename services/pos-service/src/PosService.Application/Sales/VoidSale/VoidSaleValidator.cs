using FluentValidation;

namespace PosService.Application.Sales.VoidSale;

public class VoidSaleValidator : AbstractValidator<VoidSaleCommand>
{
    public VoidSaleValidator()
    {
        RuleFor(x => x.Request.SaleId).NotEmpty().WithMessage("Sale is required.");
        RuleFor(x => x.Request.Reason).NotEmpty().WithMessage("A void reason is required.").MaximumLength(500);
    }
}
