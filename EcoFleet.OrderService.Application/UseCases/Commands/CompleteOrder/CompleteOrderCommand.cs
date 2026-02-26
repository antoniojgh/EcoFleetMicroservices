using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CompleteOrder;

public record CompleteOrderCommand(Guid Id) : IRequest;
