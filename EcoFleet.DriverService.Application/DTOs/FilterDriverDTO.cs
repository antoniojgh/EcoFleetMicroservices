using EcoFleet.DriverService.Domain.Enums;

namespace EcoFleet.DriverService.Application.DTOs;

public record FilterDriverDTO
{
    public int Page { get; init; } = 1;
    public int RecordsByPage { get; init; } = 10;
    public Guid? Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? License { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime? DateOfBirthFrom { get; init; }
    public DateTime? DateOfBirthTo { get; init; }
    public DriverStatus? Status { get; init; }
    public Guid? AssignedVehicleId { get; init; }
}
