using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.FleetService.Application.Interfaces;
using MassTransit;

namespace EcoFleet.FleetService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes DriverSuspendedIntegrationEvent published by the DriverService.
/// Unassigns all vehicles that are currently assigned to the suspended driver.
/// </summary>
public class DriverSuspendedConsumer : IConsumer<DriverSuspendedIntegrationEvent>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DriverSuspendedConsumer> _logger;

    public DriverSuspendedConsumer(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        ILogger<DriverSuspendedConsumer> logger)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverSuspendedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received DriverSuspended event for DriverId: {DriverId}. Unassigning vehicles.",
            message.DriverId);

        // Find vehicles assigned to this driver and unassign them
        var vehicles = await _vehicleRepository.GetByDriverIdAsync(message.DriverId, context.CancellationToken);

        foreach (var vehicle in vehicles)
        {
            vehicle.UnassignDriver();
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Successfully unassigned all vehicles from suspended driver {DriverId}.",
            message.DriverId);
    }
}
