using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Commands.DeleteManager;

public record DeleteManagerCommand(Guid Id) : IRequest;
