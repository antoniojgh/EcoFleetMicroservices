using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using EcoFleet.DriverService.Domain.ValueObjects;
using MassTransit;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.CreateDriver;

public class CreateDriverHandler : IRequestHandler<CreateDriverCommand, Guid>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateDriverHandler(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
    {
        var name = FullName.Create(request.FirstName, request.LastName);
        var license = DriverLicense.Create(request.License);
        var email = Email.Create(request.Email);
        var phoneNumber = request.PhoneNumber is not null ? PhoneNumber.Create(request.PhoneNumber) : null;

        Driver driver;

        if (request.AssignedVehicleId.HasValue)
        {
            // In a microservice, vehicle validation is handled asynchronously via integration events.
            // The driver is created as OnDuty with the vehicle reference stored as a primitive Guid.
            driver = new Driver(name, license, email, request.AssignedVehicleId.Value, phoneNumber, request.DateOfBirth);
        }
        else
        {
            driver = new Driver(name, license, email, phoneNumber, request.DateOfBirth);
        }

        await _driverRepository.AddAsync(driver, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish Integration Event to notify other services that a driver was created
        await _publishEndpoint.Publish(new DriverCreatedIntegrationEvent
        {
            DriverId = driver.Id.Value,
            FirstName = driver.Name.FirstName,
            LastName = driver.Name.LastName,
            License = driver.License.Value,
            Email = driver.Email.Value,
            PhoneNumber = driver.PhoneNumber?.Value,
            DateOfBirth = driver.DateOfBirth,
            OccurredOn = DateTime.UtcNow
        }, cancellationToken);

        return driver.Id.Value;
    }
}
