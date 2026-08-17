using FluentValidation;
using InventoryService.Application.Stock;

namespace InventoryService.Application.Stock;

public class CreateStockValidator : AbstractValidator<CreateStockCommand>
{
    public CreateStockValidator()
    {
        RuleFor(x => x.Request.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Request.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required.");

        RuleFor(x => x.Request.InitialQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Initial quantity cannot be negative.");

        RuleFor(x => x.Request.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative.");

        RuleFor(x => x.Request.MaxStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Max stock level cannot be negative.");

        RuleFor(x => x.Request)
            .Must(x => x.MaxStockLevel == 0 || x.InitialQuantity <= x.MaxStockLevel)
            .WithMessage("Initial quantity cannot exceed max stock level.")
            .When(x => x.Request.MaxStockLevel > 0);

        RuleFor(x => x.Request.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.")
            .When(x => x.Request.UnitCost.HasValue);
    }
}
