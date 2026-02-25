using System;
using System.Collections.Generic;
using System.Text;

namespace EcoFleet.DriverService.Domain.Events.StoreEvents
{
    public record DriverVehicleAssignedStoreEvent(
        Guid DriverId, Guid VehicleId, DateTime AssignedAt);
}
