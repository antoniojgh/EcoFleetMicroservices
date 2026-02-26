using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Infrastructure.Projections;
using Marten;
using MassTransit;

namespace EcoFleet.FleetService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes DriverSuspendedIntegrationEvent published by the DriverService.
/// Queries the Marten read model to find vehicles assigned to the suspended driver,
/// then loads each VehicleAggregate from the event store and unassigns the driver.
/// </summary>
public class DriverSuspendedConsumer : IConsumer<DriverSuspendedIntegrationEvent>
{
    private readonly IVehicleEventStore _eventStore;
    private readonly IQuerySession _querySession;
    private readonly ILogger<DriverSuspendedConsumer> _logger;

    public DriverSuspendedConsumer(
        IVehicleEventStore eventStore,
        IQuerySession querySession,
        ILogger<DriverSuspendedConsumer> logger)
    {
        _eventStore = eventStore;
        _querySession = querySession;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverSuspendedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received DriverSuspended event for DriverId: {DriverId}. Unassigning vehicles.",
            message.DriverId);

        // Query the Marten read model for vehicles assigned to this driver
        var vehicleReadModels = await _querySession.Query<VehicleReadModel>()
            .Where(v => v.CurrentDriverId == message.DriverId)
            .ToListAsync(context.CancellationToken);

        foreach (var readModel in vehicleReadModels)
        {
            var vehicle = await _eventStore.LoadAsync(readModel.Id, context.CancellationToken);

            if (vehicle is null)
            {
                _logger.LogWarning(
                    "Vehicle {VehicleId} not found in event store while processing DriverSuspended for driver {DriverId}.",
                    readModel.Id,
                    message.DriverId);
                continue;
            }

            vehicle.UnassignDriver();
            await _eventStore.SaveAsync(vehicle, context.CancellationToken);
        }

        _logger.LogInformation(
            "Successfully unassigned {Count} vehicle(s) from suspended driver {DriverId}.",
            vehicleReadModels.Count,
            message.DriverId);
    }
}
