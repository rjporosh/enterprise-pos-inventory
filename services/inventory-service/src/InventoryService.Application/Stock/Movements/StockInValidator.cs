using FluentValidation;
using InventoryService.Application.Stock;

namespace InventoryService.Application.Stock;

public class StockInValidator : AbstractValidator<StockInCommand>
{
    public StockInValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.")
            .When(x => x.UnitCost.HasValue);
    }
}
