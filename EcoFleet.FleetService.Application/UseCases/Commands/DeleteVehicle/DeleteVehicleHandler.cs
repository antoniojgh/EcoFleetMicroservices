using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Aggregates;
using EcoFleet.FleetService.Domain.Enums;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.DeleteVehicle;

public class DeleteVehicleHandler : IRequestHandler<DeleteVehicleCommand>
{
    private readonly IVehicleEventStore _eventStore;

    public DeleteVehicleHandler(IVehicleEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var vehicle = await _eventStore.LoadAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(VehicleAggregate), request.Id);

        // 2. Enforce business rule: Active vehicles cannot be deleted
        if (vehicle.Status == VehicleStatus.Active)
            throw new BusinessRuleException("Cannot delete a vehicle that is currently active. Unassign the driver first.");

        // 3. Archive the event stream — events are preserved for audit, read model is removed
        await _eventStore.DeleteAsync(request.Id, cancellationToken);
    }
}
