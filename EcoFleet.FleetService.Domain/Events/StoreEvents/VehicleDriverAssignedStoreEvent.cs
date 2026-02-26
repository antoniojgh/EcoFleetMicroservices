namespace EcoFleet.FleetService.Domain.Events.StoreEvents;

public record VehicleDriverAssignedStoreEvent(
    Guid VehicleId,
    Guid DriverId,
    DateTime AssignedAt);
