using EcoFleet.AssignmentService.Application.Interfaces;
using EcoFleet.AssignmentService.Domain.Entities;
using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.DeleteAssignment;

public class DeleteAssignmentHandler : IRequestHandler<DeleteAssignmentCommand>
{
    private readonly IAssignmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAssignmentHandler(IAssignmentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignmentId = new ManagerDriverAssignmentId(request.Id);
        var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(ManagerDriverAssignment), request.Id);

        await _repository.Delete(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
