using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.OrderEvents;
using EcoFleet.OrderService.Application.Interfaces;
using EcoFleet.OrderService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CompleteOrder;

public class CompleteOrderHandler : IRequestHandler<CompleteOrderCommand>
{
    private readonly IOrderEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public CompleteOrderHandler(IOrderEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var order = await _eventStore.LoadAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrderAggregate), request.Id);

        // 2. Execute domain logic — raises OrderCompletedStoreEvent internally
        order.Complete();

        // 3. Append the new event to the Marten event stream
        await _eventStore.SaveAsync(order, cancellationToken);

        // 4. Publish Integration Event for other microservices (e.g. NotificationService)
        await _publishEndpoint.Publish(new OrderCompletedIntegrationEvent
        {
            OrderId = order.Id,
            DriverId = order.DriverId,
            Price = order.Price,
            CompletedAt = DateTime.UtcNow,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);
    }
}
