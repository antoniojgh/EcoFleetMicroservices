using EcoFleet.AssignmentService.Application.DTOs;
using EcoFleet.BuildingBlocks.Application.Common;
using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Queries.GetAllAssignments;

public record GetAllAssignmentsQuery : FilterAssignmentDTO, IRequest<PaginatedDTO<AssignmentDetailDTO>>;
