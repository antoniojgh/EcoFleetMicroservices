using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.OrderService.Application.Interfaces;
using EcoFleet.OrderService.Domain.Aggregates;
using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.StartOrder;

public class StartOrderHandler : IRequestHandler<StartOrderCommand>
{
    private readonly IOrderEventStore _eventStore;

    public StartOrderHandler(IOrderEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task Handle(StartOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var order = await _eventStore.LoadAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrderAggregate), request.Id);

        // 2. Execute domain logic — raises OrderStartedStoreEvent internally
        order.Start();

        // 3. Append the new event to the Marten event stream
        await _eventStore.SaveAsync(order, cancellationToken);
    }
}
