using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.SuspendDriver;

public record SuspendDriverCommand(Guid Id) : IRequest;
