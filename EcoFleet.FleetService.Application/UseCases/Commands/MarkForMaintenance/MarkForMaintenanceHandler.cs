using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Entities;
using MassTransit;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.MarkForMaintenance;

/// <summary>
/// Marks a vehicle for maintenance. If the vehicle had an assigned driver,
/// publishes VehicleMaintenanceStartedIntegrationEvent so the DriverService can
/// update the driver's status back to Available — replacing the direct cross-aggregate call.
/// </summary>
public class MarkForMaintenanceHandler : IRequestHandler<MarkForMaintenanceCommand>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public MarkForMaintenanceHandler(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(MarkForMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var vehicleId = new VehicleId(request.Id);
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.Id);

        Guid? previousDriverId = vehicle.CurrentDriverId;

        vehicle.MarkForMaintenance();

        await _vehicleRepository.Update(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event so the DriverService can unassign the driver.
        // This replaces the monolith's direct driver.UnassignVehicle() cross-aggregate call.
        await _publishEndpoint.Publish(new VehicleMaintenanceStartedIntegrationEvent
        {
            VehicleId = vehicle.Id.Value,
            PreviousDriverId = previousDriverId,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);
    }
}
