using EcoFleet.Domain.Common;
using EcoFleet.Domain.Entities;

namespace EcoFleet.DriverService.Domain.Events
{
    public record DriverReinstatedEvent(
        DriverId DriverId,
        DateTime OcurredOn
    ) : IDomainEvent;
}
