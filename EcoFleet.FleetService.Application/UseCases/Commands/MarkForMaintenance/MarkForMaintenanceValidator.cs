using FluentValidation;

namespace EcoFleet.FleetService.Application.UseCases.Commands.MarkForMaintenance;

public class MarkForMaintenanceValidator : AbstractValidator<MarkForMaintenanceCommand>
{
    public MarkForMaintenanceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vehicle Id is required.");
    }
}
