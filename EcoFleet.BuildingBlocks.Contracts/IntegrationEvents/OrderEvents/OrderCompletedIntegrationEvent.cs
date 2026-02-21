namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.OrderEvents;

public record OrderCompletedIntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid DriverId { get; init; }
    public decimal Price { get; init; }
    public DateTime CompletedAt { get; init; }
    public DateTime OccurredOn { get; init; }
}
