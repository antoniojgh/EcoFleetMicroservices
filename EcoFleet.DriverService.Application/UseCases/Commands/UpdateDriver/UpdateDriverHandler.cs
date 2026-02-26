using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Aggregates;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.UpdateDriver;

public class UpdateDriverHandler : IRequestHandler<UpdateDriverCommand>
{
    private readonly IDriverEventStore _eventStore;

    public UpdateDriverHandler(IDriverEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var driver = await _eventStore.LoadAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverAggregate), request.Id);

        // 2. Apply profile updates — each raises its corresponding store event internally
        driver.UpdateName(request.FirstName, request.LastName);
        driver.UpdateLicense(request.License);
        driver.UpdateEmail(request.Email);
        driver.UpdatePhoneNumber(request.PhoneNumber);
        driver.UpdateDateOfBirth(request.DateOfBirth);

        // 3. Handle vehicle assignment changes
        // Vehicle assignment is coordinated asynchronously via integration events
        // between FleetService and DriverService. Direct assignment here updates the
        // local driver aggregate state to keep it consistent.
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

        // 4. Append all uncommitted events to the Marten event stream
        await _eventStore.SaveAsync(driver, cancellationToken);
    }
}
