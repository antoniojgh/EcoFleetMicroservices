namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;

public record VehicleDriverUnassignedIntegrationEvent
{
    public Guid VehicleId { get; init; }
    public Guid DriverId { get; init; }
    public DateTime OccurredOn { get; init; }
}
