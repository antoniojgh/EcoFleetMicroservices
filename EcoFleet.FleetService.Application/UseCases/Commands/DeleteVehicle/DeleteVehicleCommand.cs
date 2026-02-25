using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.DeleteVehicle;

public record DeleteVehicleCommand(Guid Id) : IRequest;
