using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.DeactivateAssignment;

public record DeactivateAssignmentCommand(Guid Id) : IRequest;
