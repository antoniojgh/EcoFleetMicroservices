namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;

public record VehicleMaintenanceStartedIntegrationEvent
{
    public Guid VehicleId { get; init; }
    public Guid? PreviousDriverId { get; init; }
    public DateTime OccurredOn { get; init; }
}
