using EcoFleet.AssignmentService.Application.Interfaces;
using EcoFleet.AssignmentService.Domain.Entities;
using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.AssignmentEvents;
using MassTransit;
using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.DeactivateAssignment;

public class DeactivateAssignmentHandler : IRequestHandler<DeactivateAssignmentCommand>
{
    private readonly IAssignmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeactivateAssignmentHandler(
        IAssignmentRepository repository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(DeactivateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignmentId = new ManagerDriverAssignmentId(request.Id);
        var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(ManagerDriverAssignment), request.Id);

        assignment.Deactivate();

        await _repository.Update(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish Integration Event to notify other microservices
        await _publishEndpoint.Publish(new AssignmentDeactivatedIntegrationEvent
        {
            AssignmentId = assignment.Id.Value,
            ManagerId = assignment.ManagerId,
            DriverId = assignment.DriverId,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);
    }
}
