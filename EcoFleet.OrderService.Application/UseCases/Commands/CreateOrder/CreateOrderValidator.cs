using FluentValidation;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CreateOrder;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.DriverId)
            .NotEmpty().WithMessage("Driver Id is required.");

        RuleFor(x => x.DriverFirstName)
            .NotEmpty().WithMessage("Driver first name is required.");

        RuleFor(x => x.DriverLastName)
            .NotEmpty().WithMessage("Driver last name is required.");

        RuleFor(x => x.DriverEmail)
            .NotEmpty().WithMessage("Driver email is required.")
            .EmailAddress().WithMessage("Driver email must be a valid email address.");

        RuleFor(x => x.PickUpLatitude)
            .InclusiveBetween(-90, 90).WithMessage("PickUp Latitude must be between -90 and 90.");

        RuleFor(x => x.PickUpLongitude)
            .InclusiveBetween(-180, 180).WithMessage("PickUp Longitude must be between -180 and 180.");

        RuleFor(x => x.DropOffLatitude)
            .InclusiveBetween(-90, 90).WithMessage("DropOff Latitude must be between -90 and 90.");

        RuleFor(x => x.DropOffLongitude)
            .InclusiveBetween(-180, 180).WithMessage("DropOff Longitude must be between -180 and 180.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}
