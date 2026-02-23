using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EcoFleet.DriverService.Application.UseCases.IntegrationEventHandlers;

/// <summary>
/// Handles the VehicleDriverAssignedIntegrationEvent published by the FleetService.
/// Updates the driver's AssignedVehicleId to reflect the vehicle assignment.
/// </summary>
public class VehicleAssignedToDriverHandler : IConsumer<VehicleDriverAssignedIntegrationEvent>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VehicleAssignedToDriverHandler> _logger;

    public VehicleAssignedToDriverHandler(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ILogger<VehicleAssignedToDriverHandler> logger)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VehicleDriverAssignedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received VehicleDriverAssigned event. VehicleId: {VehicleId}, DriverId: {DriverId}",
            message.VehicleId,
            message.DriverId);

        var driverId = new DriverId(message.DriverId);
        var driver = await _driverRepository.GetByIdAsync(driverId, context.CancellationToken);

        if (driver is null)
        {
            _logger.LogWarning(
                "Driver {DriverId} not found while processing VehicleDriverAssigned event for vehicle {VehicleId}.",
                message.DriverId,
                message.VehicleId);
            return;
        }

        driver.AssignVehicle(message.VehicleId);

        await _driverRepository.Update(driver, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Driver {DriverId} successfully assigned to vehicle {VehicleId}.",
            message.DriverId,
            message.VehicleId);
    }
}
