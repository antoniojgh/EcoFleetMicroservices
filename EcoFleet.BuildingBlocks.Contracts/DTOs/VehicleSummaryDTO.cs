namespace EcoFleet.BuildingBlocks.Contracts.DTOs;

// Lightweight read model for inter-service HTTP queries.
// Used when a microservice (e.g. DriverService) needs to validate
// a vehicle's existence and status without owning the vehicle data.
public record VehicleSummaryDTO
{
    public Guid Id { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    // Valid values: "Idle" | "Active" | "Maintenance"
    public string Status { get; init; } = string.Empty;
    // Null when no driver is currently assigned
    public Guid? CurrentDriverId { get; init; }
    public double? CurrentLatitude { get; init; }
    public double? CurrentLongitude { get; init; }
}
