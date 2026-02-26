using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.ReinstateDriver;

public class ReinstateDriverHandler : IRequestHandler<ReinstateDriverCommand>
{
    private readonly IDriverEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public ReinstateDriverHandler(IDriverEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(ReinstateDriverCommand request, CancellationToken ct)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var driver = await _eventStore.LoadAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(DriverAggregate), request.Id);

        // 2. Execute domain logic — raises DriverReinstatedStoreEvent internally
        driver.Reinstate();

        // 3. Append the new event to the Marten event stream
        await _eventStore.SaveAsync(driver, ct);

        // 4. Publish Integration Event to RabbitMQ for other microservices
        await _publishEndpoint.Publish(new DriverReinstatedIntegrationEvent
        {
            DriverId = driver.Id,
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            Email = driver.Email,
            OccurredOn = DateTime.UtcNow
        }, ct);
    }
}
