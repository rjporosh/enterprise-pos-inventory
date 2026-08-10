using FluentValidation;
using InventoryService.Application.Stock;

namespace InventoryService.Application.Stock;

public class DeleteStockValidator : AbstractValidator<DeleteStockCommand>
{
    public DeleteStockValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stock ID is required.");
    }
}
