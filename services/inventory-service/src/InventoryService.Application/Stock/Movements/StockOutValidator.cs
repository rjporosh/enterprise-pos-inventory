using FluentValidation;
using InventoryService.Application.Stock;

namespace InventoryService.Application.Stock;

public class StockOutValidator : AbstractValidator<StockOutCommand>
{
    public StockOutValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
