using EcoFleet.AssignmentService.Application.DTOs;
using EcoFleet.AssignmentService.Application.Interfaces;
using EcoFleet.BuildingBlocks.Application.Common;
using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Queries.GetAllAssignments;

public class GetAllAssignmentsHandler : IRequestHandler<GetAllAssignmentsQuery, PaginatedDTO<AssignmentDetailDTO>>
{
    private readonly IAssignmentRepository _repository;

    public GetAllAssignmentsHandler(IAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedDTO<AssignmentDetailDTO>> Handle(GetAllAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var assignmentsFiltered = await _repository.GetFilteredAsync(request, cancellationToken);

        var assignmentsFilteredDTO = assignmentsFiltered.Select(AssignmentDetailDTO.FromEntity);

        var paginatedResult = new PaginatedDTO<AssignmentDetailDTO>
        {
            Items = assignmentsFilteredDTO,
            TotalCount = assignmentsFilteredDTO.Count()
        };

        return paginatedResult;
    }
}
