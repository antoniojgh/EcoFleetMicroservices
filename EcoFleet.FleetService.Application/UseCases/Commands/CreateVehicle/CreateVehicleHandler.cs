using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Entities;
using EcoFleet.FleetService.Domain.ValueObjects;
using MassTransit;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.CreateVehicle;

/// <summary>
/// Handles vehicle creation using the choreography pattern to replace the monolith's
/// direct cross-aggregate call to IRepositoryDriver.
///
/// Monolith approach (tightly coupled):
///   CreateVehicleHandler → IRepositoryDriver.GetByIdAsync() → validate driver → driver.AssignVehicle()
///
/// Microservice approach (loosely coupled via integration events):
///   CreateVehicleHandler → creates vehicle with primitive Guid reference → publishes VehicleDriverAssignedIntegrationEvent
///   DriverService (VehicleDriverAssignedConsumer) → receives event → updates driver.AssignedVehicleId asynchronously
/// </summary>
public class CreateVehicleHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateVehicleHandler(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var plate = LicensePlate.Create(request.LicensePlate);
        var location = Geolocation.Create(request.Latitude, request.Longitude);

        Vehicle vehicle;

        if (request.CurrentDriverId.HasValue)
        {
            // Choreography Pattern: Create vehicle with driver reference stored as a primitive Guid.
            // No direct call to DriverService — the driver state is updated asynchronously
            // when the DriverService consumes the VehicleDriverAssignedIntegrationEvent.
            vehicle = new Vehicle(plate, location, request.CurrentDriverId.Value);
        }
        else
        {
            vehicle = new Vehicle(plate, location);
        }

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish Integration Event so other services (e.g. DriverService) can react.
        // This replaces the monolith's synchronous driver.AssignVehicle() call.
        await _publishEndpoint.Publish(new VehicleCreatedIntegrationEvent
        {
            VehicleId = vehicle.Id.Value,
            LicensePlate = vehicle.Plate.Value,
            Latitude = vehicle.CurrentLocation.Latitude,
            Longitude = vehicle.CurrentLocation.Longitude,
            CurrentDriverId = vehicle.CurrentDriverId,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);

        if (vehicle.CurrentDriverId.HasValue)
        {
            // Also publish the driver assignment event so the DriverService updates
            // the driver's AssignedVehicleId and status to OnDuty.
            await _publishEndpoint.Publish(new VehicleDriverAssignedIntegrationEvent
            {
                VehicleId = vehicle.Id.Value,
                DriverId = vehicle.CurrentDriverId.Value,
                OccurredOn = DateTime.UtcNow
            }, cancellationToken);
        }

        return vehicle.Id.Value;
    }
}
