using FluentValidation;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.CreateAssignment;

public class CreateAssignmentValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentValidator()
    {
        RuleFor(x => x.ManagerId)
            .NotEmpty().WithMessage("Manager Id is required.");

        RuleFor(x => x.DriverId)
            .NotEmpty().WithMessage("Driver Id is required.");
    }
}
