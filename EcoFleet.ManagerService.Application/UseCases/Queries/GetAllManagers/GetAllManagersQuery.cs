using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.ManagerService.Application.DTOs;
using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Queries.GetAllManagers;

public record GetAllManagersQuery : FilterManagerDTO, IRequest<PaginatedDTO<ManagerDetailDTO>>;
