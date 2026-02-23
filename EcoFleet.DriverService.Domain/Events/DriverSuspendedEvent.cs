using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.DriverService.Domain.Entities;

namespace EcoFleet.DriverService.Domain.Events
{
    public record DriverSuspendedEvent(
        DriverId DriverId,
        DateTime OcurredOn
    ) : IDomainEvent;
}
