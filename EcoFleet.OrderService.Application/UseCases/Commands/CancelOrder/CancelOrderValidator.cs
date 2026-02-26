using FluentValidation;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CancelOrder;

public class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order Id is required.");
    }
}
