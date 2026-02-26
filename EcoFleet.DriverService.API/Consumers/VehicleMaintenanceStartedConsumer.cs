using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.DriverService.Application.Interfaces;
using MassTransit;

namespace EcoFleet.DriverService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes VehicleMaintenanceStartedIntegrationEvent published by FleetService.
/// Loads the DriverAggregate from the Marten event store and raises DriverVehicleUnassignedStoreEvent
/// to set the previously assigned driver back to Available when their vehicle enters maintenance.
/// </summary>
public class VehicleMaintenanceStartedConsumer : IConsumer<VehicleMaintenanceStartedIntegrationEvent>
{
    private readonly IDriverEventStore _eventStore;
    private readonly ILogger<VehicleMaintenanceStartedConsumer> _logger;

    public VehicleMaintenanceStartedConsumer(IDriverEventStore eventStore, ILogger<VehicleMaintenanceStartedConsumer> logger)
    {
        _eventStore = eventStore;
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

        var driver = await _eventStore.LoadAsync(message.PreviousDriverId.Value, context.CancellationToken);

        if (driver is null)
        {
            _logger.LogWarning(
                "Driver {DriverId} not found in event store while processing VehicleMaintenanceStarted for vehicle {VehicleId}.",
                message.PreviousDriverId,
                message.VehicleId);
            return;
        }

        driver.UnassignVehicle();

        await _eventStore.SaveAsync(driver, context.CancellationToken);

        _logger.LogInformation(
            "Driver {DriverId} successfully unassigned after vehicle {VehicleId} entered maintenance. Status set to Available.",
            message.PreviousDriverId,
            message.VehicleId);
    }
}
