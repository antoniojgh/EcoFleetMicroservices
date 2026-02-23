using FluentValidation;

namespace EcoFleet.DriverService.Application.UseCases.Commands.UpdateDriver;

public class UpdateDriverValidator : AbstractValidator<UpdateDriverCommand>
{
    public UpdateDriverValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Driver Id is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name is too long.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name is too long.");

        RuleFor(x => x.License)
            .NotEmpty().WithMessage("Driver license is required.")
            .MaximumLength(20).WithMessage("Driver license is too long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email is too long.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number is too long.")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Date of birth cannot be in the future.")
            .When(x => x.DateOfBirth.HasValue);
    }
}
