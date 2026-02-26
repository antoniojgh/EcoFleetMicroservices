using FluentValidation;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.DeactivateAssignment;

public class DeactivateAssignmentValidator : AbstractValidator<DeactivateAssignmentCommand>
{
    public DeactivateAssignmentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Assignment Id is required.");
    }
}
