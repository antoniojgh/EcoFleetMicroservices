using FluentValidation;

namespace EcoFleet.ManagerService.Application.UseCases.Commands.DeleteManager;

public class DeleteManagerValidator : AbstractValidator<DeleteManagerCommand>
{
    public DeleteManagerValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Manager Id is required.");
    }
}
