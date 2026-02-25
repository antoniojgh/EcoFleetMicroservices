using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.FleetService.Application.DTOs;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Queries.GetAllVehicles;

public record GetAllVehiclesQuery : FilterVehicleDTO, IRequest<PaginatedDTO<VehicleDetailDTO>>;
