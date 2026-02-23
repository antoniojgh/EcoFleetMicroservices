using FluentValidation;

namespace EcoFleet.DriverService.Application.UseCases.Commands.DeleteDriver;

public class DeleteDriverValidator : AbstractValidator<DeleteDriverCommand>
{
    public DeleteDriverValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Driver Id is required.");
    }
}
