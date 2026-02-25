using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.MarkForMaintenance;

public record MarkForMaintenanceCommand(Guid Id) : IRequest;
