using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.MarkForMaintenance;

/// <summary>
/// Marks a vehicle for maintenance using event sourcing (Marten). If the vehicle had an
/// assigned driver, publishes VehicleMaintenanceStartedIntegrationEvent so the DriverService
/// can update the driver's status back to Available — replacing the direct cross-aggregate call.
/// </summary>
public class MarkForMaintenanceHandler : IRequestHandler<MarkForMaintenanceCommand>
{
    private readonly IVehicleEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public MarkForMaintenanceHandler(IVehicleEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(MarkForMaintenanceCommand request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var vehicle = await _eventStore.LoadAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(VehicleAggregate), request.Id);

        // 2. Capture previous driver ID before state mutation
        Guid? previousDriverId = vehicle.CurrentDriverId;

        // 3. Execute domain logic — raises VehicleMaintenanceStartedStoreEvent internally
        vehicle.MarkForMaintenance();

        // 4. Append the new event to the Marten event stream
        await _eventStore.SaveAsync(vehicle, cancellationToken);

        // 5. Publish Integration Event so the DriverService can unassign the driver.
        //    This replaces the monolith's direct driver.UnassignVehicle() cross-aggregate call.
        await _publishEndpoint.Publish(new VehicleMaintenanceStartedIntegrationEvent
        {
            VehicleId = vehicle.Id,
            PreviousDriverId = previousDriverId,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);
    }
}
