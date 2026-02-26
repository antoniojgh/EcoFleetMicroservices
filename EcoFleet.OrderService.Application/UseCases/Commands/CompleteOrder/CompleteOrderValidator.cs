using FluentValidation;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CompleteOrder;

public class CompleteOrderValidator : AbstractValidator<CompleteOrderCommand>
{
    public CompleteOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order Id is required.");
    }
}
