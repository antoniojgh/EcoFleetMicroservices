using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.ReinstateDriver;

public record ReinstateDriverCommand(Guid Id) : IRequest;
