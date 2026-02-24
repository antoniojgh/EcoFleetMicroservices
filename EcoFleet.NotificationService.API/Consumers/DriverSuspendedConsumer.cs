using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.NotificationService.API.Notifications;
using EcoFleet.NotificationService.API.Notifications.DTOs;
using MassTransit;

namespace EcoFleet.NotificationService.API.Consumers;

public class DriverSuspendedConsumer : IConsumer<DriverSuspendedIntegrationEvent>
{
    private readonly INotificationsService _notificationsService;
    private readonly ILogger<DriverSuspendedConsumer> _logger;

    public DriverSuspendedConsumer(INotificationsService notificationsService,
        ILogger<DriverSuspendedConsumer> logger)
    {
        _notificationsService = notificationsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverSuspendedIntegrationEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received DriverSuspended event for {DriverId}", msg.DriverId);

        await _notificationsService.SendDriverSuspendedNotification(new DriverSuspendedEventDTO
        {
            FirstName = msg.FirstName,
            LastName = msg.LastName,
            Email = msg.Email
        });
    }
}
