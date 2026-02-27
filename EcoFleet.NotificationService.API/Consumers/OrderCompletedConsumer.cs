using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.OrderEvents;
using EcoFleet.NotificationService.API.Notifications;
using EcoFleet.NotificationService.API.Notifications.DTOs;
using MassTransit;

namespace EcoFleet.NotificationService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes OrderCompletedIntegrationEvent published by the OrderService.
/// Logs the delivery confirmation for audit purposes. Can be extended to send email
/// notifications when the integration event is enriched with recipient contact information.
/// </summary>
public class OrderCompletedConsumer : IConsumer<OrderCompletedIntegrationEvent>
{
    private readonly INotificationsService _notificationsService;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(INotificationsService notificationsService,
        ILogger<OrderCompletedConsumer> logger)
    {
        _notificationsService = notificationsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedIntegrationEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received OrderCompleted event for OrderId: {OrderId}, DriverId: {DriverId}",
            msg.OrderId, msg.DriverId);

        await _notificationsService.SendOrderCompletedNotification(new OrderCompletedEventDTO
        {
            OrderId = msg.OrderId,
            DriverId = msg.DriverId,
            DriverFirstName = msg.DriverFirstName,
            DriverLastName = msg.DriverLastName,
            DriverEmail = msg.DriverEmail,
            Price = msg.Price,
            CompletedAt = msg.CompletedAt
        });
    }
}
