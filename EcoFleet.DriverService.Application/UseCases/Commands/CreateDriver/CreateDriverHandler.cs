using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.CreateDriver;

public class CreateDriverHandler : IRequestHandler<CreateDriverCommand, Guid>
{
    private readonly IDriverEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateDriverHandler(IDriverEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the aggregate via factory method — raises DriverCreatedStoreEvent internally
        var aggregate = DriverAggregate.Create(
            request.FirstName,
            request.LastName,
            request.License,
            request.Email,
            request.PhoneNumber,
            request.DateOfBirth);

        // 2. If a vehicle is being assigned at creation time, raise DriverVehicleAssignedStoreEvent
        if (request.AssignedVehicleId.HasValue)
        {
            aggregate.AssignVehicle(request.AssignedVehicleId.Value);
        }

        // 3. Append all uncommitted events to the Marten event stream
        await _eventStore.SaveAsync(aggregate, cancellationToken);

        // 4. Publish Integration Event to notify other microservices
        await _publishEndpoint.Publish(new DriverCreatedIntegrationEvent
        {
            DriverId = aggregate.Id,
            FirstName = aggregate.FirstName,
            LastName = aggregate.LastName,
            License = aggregate.License,
            Email = aggregate.Email,
            PhoneNumber = aggregate.PhoneNumber,
            DateOfBirth = aggregate.DateOfBirth,
            Status = aggregate.Status.ToString(),
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);

        return aggregate.Id;
    }
}
