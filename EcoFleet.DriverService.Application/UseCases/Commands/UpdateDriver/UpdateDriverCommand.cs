using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.UpdateDriver;

public record UpdateDriverCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string License,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    Guid? AssignedVehicleId
) : IRequest;
