using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.CreateVehicle;

/// <summary>
/// Handles vehicle creation using event sourcing (Marten) and the choreography pattern
/// to replace the monolith's direct cross-aggregate call to IRepositoryDriver.
///
/// Monolith approach (tightly coupled):
///   CreateVehicleHandler → IRepositoryDriver.GetByIdAsync() → validate driver → driver.AssignVehicle()
///
/// Microservice approach (loosely coupled via integration events):
///   CreateVehicleHandler → creates VehicleAggregate in Marten event store → publishes VehicleDriverAssignedIntegrationEvent
///   DriverService (VehicleDriverAssignedConsumer) → receives event → updates driver.AssignedVehicleId asynchronously
/// </summary>
public class CreateVehicleHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateVehicleHandler(IVehicleEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the aggregate via factory method — raises VehicleCreatedStoreEvent internally
        var aggregate = VehicleAggregate.Create(
            request.LicensePlate,
            request.Latitude,
            request.Longitude);

        // 2. If a driver is being assigned at creation time, raise VehicleDriverAssignedStoreEvent
        if (request.CurrentDriverId.HasValue)
        {
            aggregate.AssignDriver(request.CurrentDriverId.Value);
        }

        // 3. Append all uncommitted events to the Marten event stream
        await _eventStore.SaveAsync(aggregate, cancellationToken);

        // 4. Publish Integration Event to notify other microservices
        await _publishEndpoint.Publish(new VehicleCreatedIntegrationEvent
        {
            VehicleId = aggregate.Id,
            LicensePlate = aggregate.LicensePlate,
            Latitude = aggregate.Latitude,
            Longitude = aggregate.Longitude,
            CurrentDriverId = aggregate.CurrentDriverId,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);

        // 5. If a driver was assigned, also publish the assignment event
        //    so the DriverService updates the driver's AssignedVehicleId and status to OnDuty.
        if (aggregate.CurrentDriverId.HasValue)
        {
            await _publishEndpoint.Publish(new VehicleDriverAssignedIntegrationEvent
            {
                VehicleId = aggregate.Id,
                DriverId = aggregate.CurrentDriverId.Value,
                OccurredOn = DateTime.UtcNow
            }, cancellationToken);
        }

        return aggregate.Id;
    }
}
