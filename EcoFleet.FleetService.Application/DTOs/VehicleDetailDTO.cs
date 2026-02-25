using EcoFleet.FleetService.Domain.Entities;
using EcoFleet.FleetService.Domain.Enums;

namespace EcoFleet.FleetService.Application.DTOs;

public record VehicleDetailDTO(
    Guid Id,
    string LicensePlate,
    VehicleStatus Status,
    double Latitude,
    double Longitude,
    Guid? CurrentDriverId
)
{
    public static VehicleDetailDTO FromEntity(Vehicle vehicle) =>
        new(
            vehicle.Id.Value,
            vehicle.Plate.Value,
            vehicle.Status,
            vehicle.CurrentLocation.Latitude,
            vehicle.CurrentLocation.Longitude,
            vehicle.CurrentDriverId
        );
}
