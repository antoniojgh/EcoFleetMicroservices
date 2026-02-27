namespace EcoFleet.NotificationService.API.Notifications.DTOs;

public record OrderCompletedEventDTO
{
    public Guid OrderId { get; init; }
    public Guid DriverId { get; init; }
    public string DriverFirstName { get; init; } = string.Empty;
    public string DriverLastName { get; init; } = string.Empty;
    public string DriverEmail { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateTime CompletedAt { get; init; }
}
