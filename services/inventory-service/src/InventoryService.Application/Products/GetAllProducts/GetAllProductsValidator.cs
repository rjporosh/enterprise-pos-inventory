using FluentValidation;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.GetAllProducts;

public class GetAllProductsValidator : AbstractValidator<GetAllProductsQuery>
{
    public GetAllProductsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be greater than or equal to 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "name", "sku", "price", "createdat" }.Contains(sortBy.ToLower()))
            .WithMessage("SortBy must be one of: name, sku, price, createdat.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));
    }
}
