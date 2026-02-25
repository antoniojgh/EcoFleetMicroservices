using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.CreateVehicle;

/// <summary>
/// Command to create a new vehicle. Optionally assigns a driver via primitive Guid reference
/// (no cross-service strong typing). The driver assignment triggers a VehicleDriverAssignedIntegrationEvent
/// which the DriverService consumes to update its own state — replacing the old direct cross-aggregate call.
/// </summary>
public record CreateVehicleCommand(
    string LicensePlate,
    double Latitude,
    double Longitude,
    Guid? CurrentDriverId
) : IRequest<Guid>;
