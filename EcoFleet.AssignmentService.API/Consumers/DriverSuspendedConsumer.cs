using EcoFleet.AssignmentService.Application.Interfaces;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.AssignmentEvents;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using MassTransit;

namespace EcoFleet.AssignmentService.API.Consumers;

/// <summary>
/// MassTransit consumer that processes DriverSuspendedIntegrationEvent published by the DriverService.
/// Queries the AssignmentRepository to find active assignments for the suspended driver,
/// then deactivates them and publishes AssignmentDeactivatedIntegrationEvent for each.
/// </summary>
public class DriverSuspendedConsumer : IConsumer<DriverSuspendedIntegrationEvent>
{
    private readonly IAssignmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DriverSuspendedConsumer> _logger;

    public DriverSuspendedConsumer(
        IAssignmentRepository repository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<DriverSuspendedConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverSuspendedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received DriverSuspended event for DriverId: {DriverId}. Deactivating assignments.",
            message.DriverId);

        // Find all active assignments for the suspended driver
        var activeAssignments = await _repository.GetActiveByDriverIdAsync(
            message.DriverId, context.CancellationToken);

        foreach (var assignment in activeAssignments)
        {
            assignment.Deactivate();
            await _repository.Update(assignment, context.CancellationToken);

            // Publish deactivation integration event
            await _publishEndpoint.Publish(new AssignmentDeactivatedIntegrationEvent
            {
                AssignmentId = assignment.Id.Value,
                ManagerId = assignment.ManagerId,
                DriverId = assignment.DriverId,
                OccurredOn = DateTime.UtcNow
            }, context.CancellationToken);

            _logger.LogInformation(
                "Deactivated assignment {AssignmentId} for suspended driver {DriverId}.",
                assignment.Id.Value,
                message.DriverId);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Finished processing DriverSuspended for driver {DriverId}. {Count} assignment(s) deactivated.",
            message.DriverId,
            activeAssignments.Count());
    }
}
