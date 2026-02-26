using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CancelOrder;

public record CancelOrderCommand(Guid Id, string? CancellationReason = null) : IRequest;
