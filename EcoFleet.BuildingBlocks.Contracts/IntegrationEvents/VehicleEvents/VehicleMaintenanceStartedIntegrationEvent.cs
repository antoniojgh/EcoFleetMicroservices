namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;

public record VehicleMaintenanceStartedIntegrationEvent
{
    public Guid VehicleId { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    // Null when no driver was assigned at the time maintenance started
    public Guid? PreviousDriverId { get; init; }
    public DateTime OccurredOn { get; init; }
}
