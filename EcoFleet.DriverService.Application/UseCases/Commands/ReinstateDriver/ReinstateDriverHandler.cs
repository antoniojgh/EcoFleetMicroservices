using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using MassTransit;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.ReinstateDriver;

public class ReinstateDriverHandler : IRequestHandler<ReinstateDriverCommand>
{
    private readonly IDriverRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public ReinstateDriverHandler(
        IDriverRepository repository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(ReinstateDriverCommand request, CancellationToken ct)
    {
        var driverId = new DriverId(request.Id);
        var driver = await _repository.GetByIdAsync(driverId, ct);

        if (driver is null)
        {
            throw new NotFoundException(nameof(Driver), request.Id);
        }

        driver.Reinstate(); // Raises internal DriverReinstatedEvent

        await _repository.Update(driver, ct);
        await _unitOfWork.SaveChangesAsync(ct); // Saves + creates outbox message

        // Publish Integration Event to RabbitMQ (for other services)
        await _publishEndpoint.Publish(new DriverReinstatedIntegrationEvent
        {
            DriverId = driver.Id.Value,
            FirstName = driver.Name.FirstName,
            LastName = driver.Name.LastName,
            Email = driver.Email.Value,
            OccurredOn = DateTime.UtcNow
        }, ct);
    }
}
