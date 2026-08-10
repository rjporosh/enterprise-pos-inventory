using FluentValidation;
using InventoryService.Application.Stock;

namespace InventoryService.Application.Stock;

public class GetAllStocksValidator : AbstractValidator<GetAllStocksQuery>
{
    public GetAllStocksValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
    }
}
