using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Aggregates;
using MassTransit;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.UpdateVehicle;

/// <summary>
/// Handles vehicle updates using event sourcing (Marten) and integration events
/// to replace the monolith's direct cross-aggregate driver repository calls.
///
/// Monolith approach (tightly coupled):
///   UpdateVehicleHandler → IRepositoryDriver.GetByIdAsync() → validate/unassign/assign drivers
///
/// Microservice approach (choreography):
///   UpdateVehicleHandler → updates VehicleAggregate in event store → publishes VehicleUpdatedIntegrationEvent (always),
///   VehicleDriverAssignedIntegrationEvent or VehicleDriverUnassignedIntegrationEvent → DriverService reacts asynchronously.
/// </summary>
public class UpdateVehicleHandler : IRequestHandler<UpdateVehicleCommand>
{
    private readonly IVehicleEventStore _eventStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateVehicleHandler(IVehicleEventStore eventStore, IPublishEndpoint publishEndpoint)
    {
        _eventStore = eventStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var vehicle = await _eventStore.LoadAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(VehicleAggregate), request.Id);

        // 2. Apply attribute updates — each raises its corresponding store event internally
        vehicle.UpdatePlate(request.LicensePlate);
        vehicle.UpdateLocation(request.Latitude, request.Longitude);

        // 3. Handle driver assignment changes
        Guid? removedDriverId = null;
        Guid? newAssignedDriverId = null;

        if (request.CurrentDriverId.HasValue)
        {
            if (vehicle.CurrentDriverId != request.CurrentDriverId.Value)
            {
                // Track the driver being removed (if any) for the integration event
                if (vehicle.CurrentDriverId.HasValue)
                    removedDriverId = vehicle.CurrentDriverId.Value;

                vehicle.AssignDriver(request.CurrentDriverId.Value);
                newAssignedDriverId = request.CurrentDriverId.Value;
            }
        }
        else
        {
            if (vehicle.CurrentDriverId.HasValue)
            {
                removedDriverId = vehicle.CurrentDriverId.Value;
                vehicle.UnassignDriver();
            }
        }

        // 4. Append all uncommitted events to the Marten event stream
        await _eventStore.SaveAsync(vehicle, cancellationToken);

        // 5. Publish integration events AFTER saving (at-least-once delivery).
        //    DriverService consumers update driver state asynchronously — no direct cross-aggregate call needed.
        await _publishEndpoint.Publish(new VehicleUpdatedIntegrationEvent
        {
            VehicleId = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            Latitude = vehicle.Latitude,
            Longitude = vehicle.Longitude,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);

        if (removedDriverId.HasValue)
        {
            await _publishEndpoint.Publish(new VehicleDriverUnassignedIntegrationEvent
            {
                VehicleId = vehicle.Id,
                DriverId = removedDriverId.Value,
                OccurredOn = DateTime.UtcNow
            }, cancellationToken);
        }

        if (newAssignedDriverId.HasValue)
        {
            await _publishEndpoint.Publish(new VehicleDriverAssignedIntegrationEvent
            {
                VehicleId = vehicle.Id,
                DriverId = newAssignedDriverId.Value,
                OccurredOn = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
