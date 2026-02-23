using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.CreateDriver;

public record CreateDriverCommand(
    string FirstName,
    string LastName,
    string License,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    Guid? AssignedVehicleId
) : IRequest<Guid>;
