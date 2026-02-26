using EcoFleet.AssignmentService.Application.DTOs;
using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Queries.GetAssignmentById;

public record GetAssignmentByIdQuery(Guid Id) : IRequest<AssignmentDetailDTO>;
