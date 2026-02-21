namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.AssignmentEvents;

public record AssignmentCreatedIntegrationEvent
{
    public Guid AssignmentId { get; init; }
    public Guid ManagerId { get; init; }
    public Guid DriverId { get; init; }
    public DateTime AssignedDate { get; init; }
    public DateTime OccurredOn { get; init; }
}
