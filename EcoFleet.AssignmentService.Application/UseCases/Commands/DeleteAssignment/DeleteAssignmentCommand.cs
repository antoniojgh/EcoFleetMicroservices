using MediatR;

namespace EcoFleet.AssignmentService.Application.UseCases.Commands.DeleteAssignment;

public record DeleteAssignmentCommand(Guid Id) : IRequest;
