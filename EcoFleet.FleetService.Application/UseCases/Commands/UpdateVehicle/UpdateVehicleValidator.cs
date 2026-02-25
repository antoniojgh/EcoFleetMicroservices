using FluentValidation;

namespace EcoFleet.FleetService.Application.UseCases.Commands.UpdateVehicle;

public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vehicle Id is required.");

        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("License plate is required.")
            .MaximumLength(15).WithMessage("License plate must not exceed 15 characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
    }
}
