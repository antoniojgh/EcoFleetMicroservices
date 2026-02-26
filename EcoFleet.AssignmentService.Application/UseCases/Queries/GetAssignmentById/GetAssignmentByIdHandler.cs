using EcoFleet.AssignmentService.Application.DTOs;
using EcoFleet.AssignmentService.Application.Interfaces;
using EcoFleet.AssignmentService.Domain.Entities;
using EcoFleet.BuildingBlocks.Application.Exceptions;
using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Queries.GetAssignmentById;

public class GetAssignmentByIdHandler : IRequestHandler<GetAssignmentByIdQuery, AssignmentDetailDTO>
{
    private readonly IAssignmentRepository _repository;

    public GetAssignmentByIdHandler(IAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AssignmentDetailDTO> Handle(GetAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assignmentId = new ManagerDriverAssignmentId(request.Id);
        var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(ManagerDriverAssignment), request.Id);

        return AssignmentDetailDTO.FromEntity(assignment);
    }
}
