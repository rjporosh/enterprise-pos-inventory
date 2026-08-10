using FluentValidation;
using InventoryService.Application.Stock;

namespace InventoryService.Application.Stock;

public class StockAdjustmentValidator : AbstractValidator<StockAdjustmentCommand>
{
    public StockAdjustmentValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required.");

        RuleFor(x => x.QuantityChange)
            .NotEqual(0).WithMessage("Quantity change must not be zero.");
    }
}
