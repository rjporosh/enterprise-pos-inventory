using FluentValidation;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(300).WithMessage("Product name must not exceed 300 characters.");

        RuleFor(x => x.Request.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.");

        RuleFor(x => x.Request.Barcode)
            .MaximumLength(100).WithMessage("Barcode must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Barcode));

        RuleFor(x => x.Request.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Request.BrandId)
            .NotEmpty().WithMessage("Brand is required.");

        RuleFor(x => x.Request.UnitId)
            .NotEmpty().WithMessage("Unit is required.");

        RuleFor(x => x.Request.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative.");

        RuleFor(x => x.Request.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price cannot be negative.");

        RuleFor(x => x.Request.DiscountPercent)
            .GreaterThanOrEqualTo(0).WithMessage("Discount percent cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Discount percent cannot exceed 100.")
            .When(x => x.Request.DiscountPercent.HasValue);

        RuleFor(x => x.Request.TaxPercent)
            .GreaterThanOrEqualTo(0).WithMessage("Tax percent cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Tax percent cannot exceed 100.")
            .When(x => x.Request.TaxPercent.HasValue);

        RuleFor(x => x.Request.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative.");

        RuleFor(x => x.Request.MaxStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Max stock level cannot be negative.");
    }
}
