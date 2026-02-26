using EcoFleet.AssignmentService.Application.Interfaces;
using EcoFleet.AssignmentService.Domain.Entities;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.AssignmentEvents;
using MassTransit;
using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.CreateAssignment;

/// <summary>
/// Handles assignment creation using EF Core (simple CRUD pattern).
/// Manager/driver validation uses eventual consistency — the assignment is created
/// with the provided Guids without cross-service HTTP calls.
/// Publishes AssignmentCreatedIntegrationEvent to notify other microservices.
/// </summary>
public class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, Guid>
{
    private readonly IAssignmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateAssignmentHandler(
        IAssignmentRepository repository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = new ManagerDriverAssignment(request.ManagerId, request.DriverId);

        await _repository.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish Integration Event to notify other microservices
        await _publishEndpoint.Publish(new AssignmentCreatedIntegrationEvent
        {
            AssignmentId = assignment.Id.Value,
            ManagerId = assignment.ManagerId,
            DriverId = assignment.DriverId,
            AssignedDate = assignment.AssignedDate,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);

        return assignment.Id.Value;
    }
}
