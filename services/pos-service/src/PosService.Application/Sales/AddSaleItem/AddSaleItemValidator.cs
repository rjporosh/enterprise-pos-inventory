using FluentValidation;

namespace PosService.Application.Sales.AddSaleItem;

public class AddSaleItemValidator : AbstractValidator<AddSaleItemCommand>
{
    public AddSaleItemValidator()
    {
        RuleFor(x => x.Request.SaleId).NotEmpty().WithMessage("Sale is required.");
        RuleFor(x => x.Request.ProductId).NotEmpty().WithMessage("Product is required.");
        RuleFor(x => x.Request.ProductName).NotEmpty().WithMessage("Product name is required.").MaximumLength(300);
        RuleFor(x => x.Request.Sku).NotEmpty().WithMessage("SKU is required.").MaximumLength(100);
        RuleFor(x => x.Request.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
        RuleFor(x => x.Request.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.Request.DiscountAmount).GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.");
        RuleFor(x => x.Request.TaxAmount).GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative.");
    }
}
