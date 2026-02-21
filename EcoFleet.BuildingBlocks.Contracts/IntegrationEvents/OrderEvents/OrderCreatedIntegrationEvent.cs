namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.OrderEvents;

public record OrderCreatedIntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid DriverId { get; init; }
    public double PickUpLatitude { get; init; }
    public double PickUpLongitude { get; init; }
    public double DropOffLatitude { get; init; }
    public double DropOffLongitude { get; init; }
    public decimal Price { get; init; }
    // Always "Pending" at creation. Valid values: "Pending" | "InProgress" | "Completed" | "Cancelled"
    public string Status { get; init; } = "Pending";
    public DateTime CreatedDate { get; init; }
    public DateTime OccurredOn { get; init; }
}
