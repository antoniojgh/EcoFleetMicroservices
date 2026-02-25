using System;
using System.Collections.Generic;
using System.Text;

namespace EcoFleet.DriverService.Domain.Events.StoreEvents
{
    public record DriverNameUpdatedStoreEvent(
        Guid DriverId, string FirstName, string LastName, DateTime UpdatedAt);
}
