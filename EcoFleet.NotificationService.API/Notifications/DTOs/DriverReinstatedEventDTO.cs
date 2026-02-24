namespace EcoFleet.NotificationService.API.Notifications.DTOs;

public record DriverReinstatedEventDTO
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
