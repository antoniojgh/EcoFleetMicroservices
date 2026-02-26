using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.DriverService.Application.Interfaces;
using MassTransit;

namespace EcoFleet.DriverService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes VehicleDriverUnassignedIntegrationEvent published by FleetService.
/// Loads the DriverAggregate from the Marten event store and raises DriverVehicleUnassignedStoreEvent
/// to update the driver's state back to Available when they are manually unassigned from a vehicle.
/// </summary>
public class VehicleDriverUnassignedConsumer : IConsumer<VehicleDriverUnassignedIntegrationEvent>
{
    private readonly IDriverEventStore _eventStore;
    private readonly ILogger<VehicleDriverUnassignedConsumer> _logger;

    public VehicleDriverUnassignedConsumer(IDriverEventStore eventStore, ILogger<VehicleDriverUnassignedConsumer> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VehicleDriverUnassignedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received VehicleDriverUnassigned event. VehicleId: {VehicleId}, DriverId: {DriverId}",
            message.VehicleId,
            message.DriverId);

        var driver = await _eventStore.LoadAsync(message.DriverId, context.CancellationToken);

        if (driver is null)
        {
            _logger.LogWarning(
                "Driver {DriverId} not found in event store while processing VehicleDriverUnassigned for vehicle {VehicleId}.",
                message.DriverId,
                message.VehicleId);
            return;
        }

        driver.UnassignVehicle();

        await _eventStore.SaveAsync(driver, context.CancellationToken);

        _logger.LogInformation(
            "Driver {DriverId} successfully unassigned from vehicle {VehicleId}. Status set to Available.",
            message.DriverId,
            message.VehicleId);
    }
}
