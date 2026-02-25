using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using MassTransit;

namespace EcoFleet.DriverService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes VehicleMaintenanceStartedIntegrationEvent published by the FleetService.
/// Updates the previously assigned driver's state back to Available when their vehicle enters maintenance.
/// This replaces the monolith's direct driver.UnassignVehicle() cross-aggregate call in MarkForMaintenanceHandler.
/// </summary>
public class VehicleMaintenanceStartedConsumer : IConsumer<VehicleMaintenanceStartedIntegrationEvent>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VehicleMaintenanceStartedConsumer> _logger;

    public VehicleMaintenanceStartedConsumer(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ILogger<VehicleMaintenanceStartedConsumer> logger)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VehicleMaintenanceStartedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received VehicleMaintenanceStarted event for VehicleId: {VehicleId}. PreviousDriverId: {DriverId}",
            message.VehicleId,
            message.PreviousDriverId);

        if (message.PreviousDriverId is null)
        {
            _logger.LogInformation(
                "No driver was assigned to vehicle {VehicleId} before maintenance. No driver update needed.",
                message.VehicleId);
            return;
        }

        var driverId = new DriverId(message.PreviousDriverId.Value);
        var driver = await _driverRepository.GetByIdAsync(driverId, context.CancellationToken);

        if (driver is null)
        {
            _logger.LogWarning(
                "Driver {DriverId} not found while processing VehicleMaintenanceStarted event for vehicle {VehicleId}.",
                message.PreviousDriverId,
                message.VehicleId);
            return;
        }

        driver.UnassignVehicle();

        await _driverRepository.Update(driver, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Driver {DriverId} successfully unassigned after vehicle {VehicleId} entered maintenance. Status set to Available.",
            message.PreviousDriverId,
            message.VehicleId);
    }
}
