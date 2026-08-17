using FluentValidation;

namespace PosService.Application.Sales.GetAllSales;

public class GetAllSalesValidator : AbstractValidator<GetAllSalesQuery>
{
    public GetAllSalesValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("Page number must be greater than or equal to 1.");
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100).WithMessage("Page size must be between 1 and 100.");
    }
}
