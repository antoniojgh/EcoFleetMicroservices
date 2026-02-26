using FluentValidation;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.DeleteAssignment;

public class DeleteAssignmentValidator : AbstractValidator<DeleteAssignmentCommand>
{
    public DeleteAssignmentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Assignment Id is required.");
    }
}
