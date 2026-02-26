using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.OrderEvents;
using EcoFleet.OrderService.Application.Interfaces;
using EcoFleet.OrderService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CancelOrder;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelOrderHandler(IOrderEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var order = await _eventStore.LoadAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrderAggregate), request.Id);

        // 2. Execute domain logic — raises OrderCancelledStoreEvent internally
        order.Cancel(request.CancellationReason);

        // 3. Append the new event to the Marten event stream
        await _eventStore.SaveAsync(order, cancellationToken);

        // 4. Publish Integration Event for other microservices
        await _publishEndpoint.Publish(new OrderCancelledIntegrationEvent
        {
            OrderId = order.Id,
            DriverId = order.DriverId,
            CancellationReason = request.CancellationReason,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);
    }
}
