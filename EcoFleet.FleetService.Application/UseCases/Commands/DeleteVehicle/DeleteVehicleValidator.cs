using FluentValidation;

namespace EcoFleet.FleetService.Application.UseCases.Commands.DeleteVehicle;

public class DeleteVehicleValidator : AbstractValidator<DeleteVehicleCommand>
{
    public DeleteVehicleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vehicle Id is required.");
    }
}
