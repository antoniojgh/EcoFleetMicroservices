using System;
using System.Collections.Generic;
using System.Text;

namespace EcoFleet.DriverService.Domain.Events.StoreEvents
{
    public record DriverSuspendedStoreEvent(
        Guid DriverId, DateTime SuspendedAt);
}
