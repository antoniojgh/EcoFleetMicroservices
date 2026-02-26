using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;
using EcoFleet.DriverService.Application.Interfaces;
using MassTransit;

namespace EcoFleet.DriverService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes VehicleDriverAssignedIntegrationEvent published by FleetService.
/// Loads the DriverAggregate from the Marten event store and raises DriverVehicleAssignedStoreEvent
/// to keep the driver's state consistent with the vehicle assignment in FleetService.
/// </summary>
public class VehicleDriverAssignedConsumer : IConsumer<VehicleDriverAssignedIntegrationEvent>
{
    private readonly IDriverEventStore _eventStore;
    private readonly ILogger<VehicleDriverAssignedConsumer> _logger;

    public VehicleDriverAssignedConsumer(IDriverEventStore eventStore, ILogger<VehicleDriverAssignedConsumer> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VehicleDriverAssignedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received VehicleDriverAssigned event. VehicleId: {VehicleId}, DriverId: {DriverId}",
            message.VehicleId,
            message.DriverId);

        var driver = await _eventStore.LoadAsync(message.DriverId, context.CancellationToken);

        if (driver is null)
        {
            _logger.LogWarning(
                "Driver {DriverId} not found in event store while processing VehicleDriverAssigned for vehicle {VehicleId}.",
                message.DriverId,
                message.VehicleId);
            return;
        }

        driver.AssignVehicle(message.VehicleId);

        await _eventStore.SaveAsync(driver, context.CancellationToken);

        _logger.LogInformation(
            "Driver {DriverId} successfully assigned to vehicle {VehicleId}.",
            message.DriverId,
            message.VehicleId);
    }
}
