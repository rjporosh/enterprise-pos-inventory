using FluentValidation;

namespace PosService.Application.Stores.CreateStore;

public class CreateStoreValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200).WithMessage("Store name is required.");
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(50).WithMessage("Store code is required.");
        RuleFor(x => x.Request.Currency).NotEmpty().MaximumLength(3).WithMessage("Currency is required.");
        RuleFor(x => x.Request.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Request.Email))
            .WithMessage("Email must be a valid email address.");
    }
}
