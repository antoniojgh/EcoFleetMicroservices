using EcoFleet.ManagerService.Application.DTOs;
using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Queries.GetManagerById;

public record GetManagerByIdQuery(Guid Id) : IRequest<ManagerDetailDTO>;
