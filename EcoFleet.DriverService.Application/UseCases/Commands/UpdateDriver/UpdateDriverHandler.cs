using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using EcoFleet.DriverService.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.UpdateDriver;

public class UpdateDriverHandler : IRequestHandler<UpdateDriverCommand>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDriverHandler(IDriverRepository driverRepository, IUnitOfWork unitOfWork)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
    {
        var driverId = new DriverId(request.Id);
        var driver = await _driverRepository.GetByIdAsync(driverId, cancellationToken);

        if (driver is null)
            throw new NotFoundException(nameof(Driver), request.Id);

        var name = FullName.Create(request.FirstName, request.LastName);
        var license = DriverLicense.Create(request.License);
        var email = Email.Create(request.Email);
        var phoneNumber = request.PhoneNumber is not null ? PhoneNumber.Create(request.PhoneNumber) : null;

        driver.UpdateName(name);
        driver.UpdateLicense(license);
        driver.UpdateEmail(email);
        driver.UpdatePhoneNumber(phoneNumber);
        driver.UpdateDateOfBirth(request.DateOfBirth);

        // In a microservice architecture, vehicle assignment/unassignment is coordinated
        // via integration events rather than direct cross-service calls.
        // The driver's AssignedVehicleId is updated locally; the FleetService listens
        // for driver events to keep its own state consistent.
        if (request.AssignedVehicleId.HasValue)
        {
            if (driver.AssignedVehicleId != request.AssignedVehicleId.Value)
            {
                driver.AssignVehicle(request.AssignedVehicleId.Value);
            }
        }
        else
        {
            if (driver.AssignedVehicleId is not null)
            {
                driver.UnassignVehicle();
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
