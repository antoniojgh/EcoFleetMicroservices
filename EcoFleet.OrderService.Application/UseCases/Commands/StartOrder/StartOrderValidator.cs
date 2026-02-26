using FluentValidation;

namespace EcoFleet.OrderService.Application.UseCases.Commands.StartOrder;

public class StartOrderValidator : AbstractValidator<StartOrderCommand>
{
    public StartOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order Id is required.");
    }
}
