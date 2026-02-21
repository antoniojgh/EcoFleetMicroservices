using EcoFleet.Domain.Common;
using EcoFleet.Domain.Entities;

namespace EcoFleet.DriverService.Domain.Events
{
    public record DriverSuspendedEvent(
        DriverId DriverId,
        DateTime OcurredOn
    ) : IDomainEvent;
}
