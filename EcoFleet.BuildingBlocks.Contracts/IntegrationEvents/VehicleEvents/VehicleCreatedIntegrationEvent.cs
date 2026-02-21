namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;

public record VehicleCreatedIntegrationEvent
{
    public Guid VehicleId { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public double InitialLatitude { get; init; }
    public double InitialLongitude { get; init; }
    // Null when vehicle is created without an assigned driver
    public Guid? AssignedDriverId { get; init; }
    // Always "Idle" at creation. Valid values: "Idle" | "Active" | "Maintenance"
    public string Status { get; init; } = "Idle";
    public DateTime OccurredOn { get; init; }
}
