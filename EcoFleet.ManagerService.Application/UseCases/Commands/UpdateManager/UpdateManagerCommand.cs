using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Commands.UpdateManager;

public record UpdateManagerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
) : IRequest;
