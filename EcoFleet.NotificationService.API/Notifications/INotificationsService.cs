using EcoFleet.NotificationService.API.Notifications.DTOs;

namespace EcoFleet.NotificationService.API.Notifications;

public interface INotificationsService
{
    Task SendDriverSuspendedNotification(DriverSuspendedEventDTO eventDTO);
    Task SendDriverReinstatedNotification(DriverReinstatedEventDTO eventDTO);
}
