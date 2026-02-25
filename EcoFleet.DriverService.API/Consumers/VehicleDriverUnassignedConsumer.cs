using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using MassTransit;

namespace EcoFleet.DriverService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes VehicleDriverUnassignedIntegrationEvent published by the FleetService.
/// Updates the driver's state back to Available when they are manually unassigned from a vehicle.
/// This replaces the monolith's direct driver.UnassignVehicle() cross-aggregate call in UpdateVehicleHandler.
/// </summary>
public class VehicleDriverUnassignedConsumer : IConsumer<VehicleDriverUnassignedIntegrationEvent>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VehicleDriverUnassignedConsumer> _logger;

    public VehicleDriverUnassignedConsumer(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ILogger<VehicleDriverUnassignedConsumer> logger)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VehicleDriverUnassignedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received VehicleDriverUnassigned event. VehicleId: {VehicleId}, DriverId: {DriverId}",
            message.VehicleId,
            message.DriverId);

        var driverId = new DriverId(message.DriverId);
        var driver = await _driverRepository.GetByIdAsync(driverId, context.CancellationToken);

        if (driver is null)
        {
            _logger.LogWarning(
                "Driver {DriverId} not found while processing VehicleDriverUnassigned event for vehicle {VehicleId}.",
                message.DriverId,
                message.VehicleId);
            return;
        }

        driver.UnassignVehicle();

        await _driverRepository.Update(driver, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Driver {DriverId} successfully unassigned from vehicle {VehicleId}. Status set to Available.",
            message.DriverId,
            message.VehicleId);
    }
}
