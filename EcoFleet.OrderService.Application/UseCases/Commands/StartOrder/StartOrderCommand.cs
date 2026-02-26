using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.StartOrder;

public record StartOrderCommand(Guid Id) : IRequest;
