namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.VehicleEvents;

public record VehicleUpdatedIntegrationEvent
{
    public Guid VehicleId { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public DateTime OccurredOn { get; init; }
}
