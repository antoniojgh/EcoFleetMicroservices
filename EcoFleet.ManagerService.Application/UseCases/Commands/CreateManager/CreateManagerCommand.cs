using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Commands.CreateManager;

public record CreateManagerCommand(
    string FirstName,
    string LastName,
    string Email
) : IRequest<Guid>;
