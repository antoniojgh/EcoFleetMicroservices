using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.UpdateVehicle;

public record UpdateVehicleCommand(
    Guid Id,
    string LicensePlate,
    double Latitude,
    double Longitude,
    Guid? CurrentDriverId
) : IRequest;
