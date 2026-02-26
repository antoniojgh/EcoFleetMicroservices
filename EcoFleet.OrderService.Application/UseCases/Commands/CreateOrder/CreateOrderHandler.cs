using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.OrderEvents;
using EcoFleet.OrderService.Application.Interfaces;
using EcoFleet.OrderService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderHandler(IOrderEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the aggregate via factory method — raises OrderCreatedStoreEvent internally
        var aggregate = OrderAggregate.Create(
            request.DriverId,
            request.PickUpLatitude,
            request.PickUpLongitude,
            request.DropOffLatitude,
            request.DropOffLongitude,
            request.Price);

        // 2. Append all uncommitted events to the Marten event stream
        await _eventStore.SaveAsync(aggregate, cancellationToken);

        // 3. Publish Integration Event to notify other microservices
        await _publishEndpoint.Publish(new OrderCreatedIntegrationEvent
        {
            OrderId = aggregate.Id,
            DriverId = aggregate.DriverId,
            PickUpLatitude = aggregate.PickUpLatitude,
            PickUpLongitude = aggregate.PickUpLongitude,
            DropOffLatitude = aggregate.DropOffLatitude,
            DropOffLongitude = aggregate.DropOffLongitude,
            Price = aggregate.Price,
            Status = aggregate.Status.ToString(),
            CreatedDate = aggregate.CreatedDate,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);

        return aggregate.Id;
    }
}
