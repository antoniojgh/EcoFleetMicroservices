using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.CreateAssignment;

/// <summary>
/// Command to create a new manager-driver assignment. ManagerId and DriverId are primitive Guids
/// (no cross-boundary strong typing). Manager/driver validation uses eventual consistency.
/// </summary>
public record CreateAssignmentCommand(
    Guid ManagerId,
    Guid DriverId
) : IRequest<Guid>;
