using FluentValidation;

namespace EcoFleet.FleetService.Application.UseCases.Commands.CreateVehicle;

public class CreateVehicleValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("License plate is required.")
            .MaximumLength(15).WithMessage("License plate must not exceed 15 characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
    }
}
