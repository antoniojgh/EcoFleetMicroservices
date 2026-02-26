namespace EcoFleet.NotificationService.API.Notifications.DTOs;

public record OrderCompletedEventDTO
{
    public Guid OrderId { get; init; }
    public Guid DriverId { get; init; }
    public decimal Price { get; init; }
    public DateTime CompletedAt { get; init; }
}
