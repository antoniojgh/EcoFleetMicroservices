using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.NotificationService.API.Notifications;
using EcoFleet.NotificationService.API.Notifications.DTOs;
using MassTransit;

namespace EcoFleet.NotificationService.API.Consumers;

public class DriverReinstatedConsumer : IConsumer<DriverReinstatedIntegrationEvent>
{
    private readonly INotificationsService _notificationsService;
    private readonly ILogger<DriverReinstatedConsumer> _logger;

    public DriverReinstatedConsumer(INotificationsService notificationsService,
        ILogger<DriverReinstatedConsumer> logger)
    {
        _notificationsService = notificationsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverReinstatedIntegrationEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received DriverReinstatement event for {DriverId}", msg.DriverId);

        await _notificationsService.SendDriverReinstatedNotification(new DriverReinstatedEventDTO
        {
            FirstName = msg.FirstName,
            LastName = msg.LastName,
            Email = msg.Email
        });
    }
}
