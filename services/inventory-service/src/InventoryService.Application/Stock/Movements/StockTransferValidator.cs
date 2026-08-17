using FluentValidation;
using InventoryService.Application.Stock;

namespace InventoryService.Application.Stock;

public class StockTransferValidator : AbstractValidator<StockTransferCommand>
{
    public StockTransferValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.FromWarehouseId)
            .NotEmpty().WithMessage("Source warehouse ID is required.");

        RuleFor(x => x.ToWarehouseId)
            .NotEmpty().WithMessage("Destination warehouse ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.ToWarehouseId)
            .NotEqual(x => x.FromWarehouseId)
            .WithMessage("Source and destination warehouses must be different.");
    }
}
