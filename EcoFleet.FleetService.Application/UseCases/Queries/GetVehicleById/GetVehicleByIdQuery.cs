using EcoFleet.FleetService.Application.DTOs;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Queries.GetVehicleById;

public record GetVehicleByIdQuery(Guid Id) : IRequest<VehicleDetailDTO>;
