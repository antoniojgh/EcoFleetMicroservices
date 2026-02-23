using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.DriverService.Application.DTOs;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Queries.GetAllDrivers;

public record GetAllDriversQuery : FilterDriverDTO, IRequest<PaginatedDTO<DriverDetailDTO>>;
