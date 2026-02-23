using FluentValidation;

namespace EcoFleet.DriverService.Application.UseCases.Commands.SuspendDriver;

public class SuspendDriverValidator : AbstractValidator<SuspendDriverCommand>
{
    public SuspendDriverValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Driver Id is required.");
    }
}
