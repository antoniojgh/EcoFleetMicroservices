using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Entities;
using EcoFleet.FleetService.Domain.ValueObjects;
using MassTransit;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.UpdateVehicle;

/// <summary>
/// Handles vehicle updates using integration events to replace the monolith's
/// direct cross-aggregate driver repository calls.
///
/// Monolith approach (tightly coupled):
///   UpdateVehicleHandler → IRepositoryDriver.GetByIdAsync() → validate/unassign/assign drivers
///
/// Microservice approach (choreography):
///   UpdateVehicleHandler → updates vehicle state → saves → publishes VehicleUpdatedIntegrationEvent (always),
///   VehicleDriverAssignedIntegrationEvent or VehicleDriverUnassignedIntegrationEvent → DriverService reacts asynchronously.
/// </summary>
public class UpdateVehicleHandler : IRequestHandler<UpdateVehicleCommand>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateVehicleHandler(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicleId = new VehicleId(request.Id);
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.Id);

        var plate = LicensePlate.Create(request.LicensePlate);
        var location = Geolocation.Create(request.Latitude, request.Longitude);

        vehicle.UpdatePlate(plate);
        vehicle.UpdateTelemetry(location);

        // Capture previous driver ID before any state mutation
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

        await _vehicleRepository.Update(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration events AFTER saving (at-least-once delivery).
        // DriverService consumers update driver state asynchronously — no direct cross-aggregate call needed.

        // Always publish for non-driver attribute changes (plate, location).
        await _publishEndpoint.Publish(new VehicleUpdatedIntegrationEvent
        {
            VehicleId = vehicle.Id.Value,
            LicensePlate = vehicle.Plate.Value,
            Latitude = vehicle.CurrentLocation.Latitude,
            Longitude = vehicle.CurrentLocation.Longitude,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);

        if (removedDriverId.HasValue)
        {
            await _publishEndpoint.Publish(new VehicleDriverUnassignedIntegrationEvent
            {
                VehicleId = vehicle.Id.Value,
                DriverId = removedDriverId.Value,
                OccurredOn = DateTime.UtcNow
            }, cancellationToken);
        }

        if (newAssignedDriverId.HasValue)
        {
            await _publishEndpoint.Publish(new VehicleDriverAssignedIntegrationEvent
            {
                VehicleId = vehicle.Id.Value,
                DriverId = newAssignedDriverId.Value,
                OccurredOn = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
