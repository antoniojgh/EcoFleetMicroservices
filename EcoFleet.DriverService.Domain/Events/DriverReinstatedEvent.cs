using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.DriverService.Domain.Entities;

namespace EcoFleet.DriverService.Domain.Events
{
    public record DriverReinstatedEvent(
        DriverId DriverId,
        DateTime OcurredOn
    ) : IDomainEvent;
}
