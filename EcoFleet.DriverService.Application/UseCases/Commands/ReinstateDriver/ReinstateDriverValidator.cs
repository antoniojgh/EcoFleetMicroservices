using FluentValidation;

namespace EcoFleet.DriverService.Application.UseCases.Commands.ReinstateDriver;

public class ReinstateDriverValidator : AbstractValidator<ReinstateDriverCommand>
{
    public ReinstateDriverValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Driver Id is required.");
    }
}
