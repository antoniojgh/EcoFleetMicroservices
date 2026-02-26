using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Aggregates;
using EcoFleet.DriverService.Domain.Enums;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.DeleteDriver;

public class DeleteDriverHandler : IRequestHandler<DeleteDriverCommand>
{
    private readonly IDriverEventStore _eventStore;

    public DeleteDriverHandler(IDriverEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate by replaying its event stream from Marten
        var driver = await _eventStore.LoadAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverAggregate), request.Id);

        // 2. Enforce business rule: OnDuty drivers cannot be deleted
        if (driver.Status == DriverStatus.OnDuty)
        {
            throw new BusinessRuleException("Cannot delete a driver who is currently on duty. Unassign them first.");
        }

        // 3. Archive the event stream — events are preserved for audit, read model is removed
        await _eventStore.DeleteAsync(request.Id, cancellationToken);
    }
}
