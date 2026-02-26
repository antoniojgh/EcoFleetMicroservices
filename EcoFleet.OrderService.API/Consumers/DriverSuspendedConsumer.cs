using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.OrderEvents;
using EcoFleet.OrderService.Application.Interfaces;
using EcoFleet.OrderService.Domain.Enums;
using EcoFleet.OrderService.Infrastructure.Projections;
using Marten;
using MassTransit;

namespace EcoFleet.OrderService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes DriverSuspendedIntegrationEvent published by the DriverService.
/// Queries the Marten read model to find pending orders assigned to the suspended driver,
/// then loads each OrderAggregate from the event store and auto-cancels them.
/// </summary>
public class DriverSuspendedConsumer : IConsumer<DriverSuspendedIntegrationEvent>
{
    private readonly IOrderEventStore _eventStore;
    private readonly IQuerySession _querySession;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DriverSuspendedConsumer> _logger;

    public DriverSuspendedConsumer(
        IOrderEventStore eventStore,
        IQuerySession querySession,
        IPublishEndpoint publishEndpoint,
        ILogger<DriverSuspendedConsumer> logger)
    {
        _eventStore = eventStore;
        _querySession = querySession;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverSuspendedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received DriverSuspended event for DriverId: {DriverId}. Auto-cancelling pending orders.",
            message.DriverId);

        // Query the Marten read model for pending orders assigned to this driver
        var pendingOrders = await _querySession.Query<OrderReadModel>()
            .Where(o => o.DriverId == message.DriverId && o.Status == OrderStatus.Pending.ToString())
            .ToListAsync(context.CancellationToken);

        foreach (var readModel in pendingOrders)
        {
            var order = await _eventStore.LoadAsync(readModel.Id, context.CancellationToken);

            if (order is null)
            {
                _logger.LogWarning(
                    "Order {OrderId} not found in event store while processing DriverSuspended for driver {DriverId}.",
                    readModel.Id,
                    message.DriverId);
                continue;
            }

            order.Cancel("DriverSuspended");
            await _eventStore.SaveAsync(order, context.CancellationToken);

            // Publish cancellation integration event
            await _publishEndpoint.Publish(new OrderCancelledIntegrationEvent
            {
                OrderId = order.Id,
                DriverId = order.DriverId,
                CancellationReason = "DriverSuspended",
                OccurredOn = DateTime.UtcNow
            }, context.CancellationToken);

            _logger.LogInformation(
                "Auto-cancelled order {OrderId} for suspended driver {DriverId}.",
                readModel.Id,
                message.DriverId);
        }

        _logger.LogInformation(
            "Finished processing DriverSuspended for driver {DriverId}. {Count} order(s) cancelled.",
            message.DriverId,
            pendingOrders.Count);
    }
}
