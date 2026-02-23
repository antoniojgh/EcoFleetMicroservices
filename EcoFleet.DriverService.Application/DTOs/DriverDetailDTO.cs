using EcoFleet.DriverService.Domain.Entities;
using EcoFleet.DriverService.Domain.Enums;

namespace EcoFleet.DriverService.Application.DTOs;

public record DriverDetailDTO(
    Guid Id,
    string FirstName,
    string LastName,
    string License,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    DriverStatus Status,
    Guid? AssignedVehicleId
)
{
    public static DriverDetailDTO FromEntity(Driver driver) =>
        new(
            driver.Id.Value,
            driver.Name.FirstName,
            driver.Name.LastName,
            driver.License.Value,
            driver.Email.Value,
            driver.PhoneNumber?.Value,
            driver.DateOfBirth,
            driver.Status,
            driver.AssignedVehicleId
        );
}
