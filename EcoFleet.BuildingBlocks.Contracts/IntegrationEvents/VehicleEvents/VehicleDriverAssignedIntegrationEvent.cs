namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;

public record VehicleDriverAssignedIntegrationEvent
{
    public Guid VehicleId { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public Guid DriverId { get; init; }
    // Always "Active" after a driver is assigned. Valid values: "Idle" | "Active" | "Maintenance"
    public string VehicleStatus { get; init; } = "Active";
    public DateTime OccurredOn { get; init; }
}
