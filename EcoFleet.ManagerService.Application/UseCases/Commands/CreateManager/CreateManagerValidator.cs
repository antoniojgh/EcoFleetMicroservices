using FluentValidation;

namespace EcoFleet.ManagerService.Application.UseCases.Commands.CreateManager;

public class CreateManagerValidator : AbstractValidator<CreateManagerCommand>
{
    public CreateManagerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name is too long.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name is too long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email is too long.")
            .EmailAddress().WithMessage("Email format is invalid.");
    }
}
