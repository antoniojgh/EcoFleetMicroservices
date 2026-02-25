using EcoFleet.FleetService.Domain.Enums;

namespace EcoFleet.FleetService.Application.DTOs;

public record FilterVehicleDTO
{
    public int Page { get; init; } = 1;
    public int RecordsByPage { get; init; } = 10;
    public Guid? Id { get; init; }
    public string? Plate { get; init; }
    public VehicleStatus? Status { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public Guid? CurrentDriverId { get; init; }
}
