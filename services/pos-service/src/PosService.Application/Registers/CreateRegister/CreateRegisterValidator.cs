using FluentValidation;

namespace PosService.Application.Registers.CreateRegister;

public class CreateRegisterValidator : AbstractValidator<CreateRegisterCommand>
{
    public CreateRegisterValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200).WithMessage("Register name is required.");
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(50).WithMessage("Register code is required.");
        RuleFor(x => x.Request.StoreId).NotEmpty().WithMessage("Store is required.");
    }
}
