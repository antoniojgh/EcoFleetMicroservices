using System;
using System.Collections.Generic;
using System.Text;

namespace EcoFleet.DriverService.Domain.Events.StoreEvents
{
    public record DriverCreatedStoreEvent(
        Guid DriverId, string FirstName, string LastName,
        string License, string Email, string? PhoneNumber,
        DateTime? DateOfBirth, DateTime CreatedAt);
}
