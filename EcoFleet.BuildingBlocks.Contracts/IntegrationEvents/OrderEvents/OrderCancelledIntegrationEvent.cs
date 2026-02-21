namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.OrderEvents;

public record OrderCancelledIntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid DriverId { get; init; }
    // Describes why the order was cancelled, e.g. "DriverSuspended" | "ManualCancellation"
    public string? CancellationReason { get; init; }
    public DateTime OccurredOn { get; init; }
}
