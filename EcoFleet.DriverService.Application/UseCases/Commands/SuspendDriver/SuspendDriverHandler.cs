using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.SuspendDriver;

public class SuspendDriverHandler : IRequestHandler<SuspendDriverCommand>
{
    private readonly IDriverEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public SuspendDriverHandler(IDriverEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(SuspendDriverCommand request, CancellationToken ct)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var driver = await _eventStore.LoadAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(DriverAggregate), request.Id);

        // 2. Execute domain logic — raises DriverSuspendedStoreEvent internally
        driver.Suspend();

        // 3. Append the new event to the Marten event stream
        await _eventStore.SaveAsync(driver, ct);

        // 4. Publish Integration Event to RabbitMQ for other microservices
        await _publishEndpoint.Publish(new DriverSuspendedIntegrationEvent
        {
            DriverId = driver.Id,
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            License = driver.License,
            Email = driver.Email,
            OccurredOn = DateTime.UtcNow
        }, ct);
    }
}
