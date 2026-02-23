using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.DeleteDriver;

public record DeleteDriverCommand(Guid Id) : IRequest;