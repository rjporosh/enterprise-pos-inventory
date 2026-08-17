using FluentValidation;
using InventoryService.Application.Stock;

namespace InventoryService.Application.Stock;

public class UpdateStockValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockValidator()
    {
        RuleFor(x => x.Request.Id)
            .NotEmpty().WithMessage("Stock ID is required.");

        RuleFor(x => x.Request.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Request.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required.");

        RuleFor(x => x.Request.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative.");

        RuleFor(x => x.Request.MaxStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Max stock level cannot be negative.");
    }
}
