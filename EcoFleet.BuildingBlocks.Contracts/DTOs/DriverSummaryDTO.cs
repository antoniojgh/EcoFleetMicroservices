namespace EcoFleet.BuildingBlocks.Contracts.DTOs;

// Lightweight read model for inter-service HTTP queries.
// Used when a microservice (e.g. FleetService) needs to validate
// a driver's existence and status without owning the driver data.
public record DriverSummaryDTO
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string LicenseNumber { get; init; } = string.Empty;
    // Valid values: "Available" | "OnDuty" | "Suspended"
    public string Status { get; init; } = string.Empty;
    // Null when the driver is not assigned to any vehicle
    public Guid? AssignedVehicleId { get; init; }
}
