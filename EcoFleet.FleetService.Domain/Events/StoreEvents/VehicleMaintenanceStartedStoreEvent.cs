namespace EcoFleet.FleetService.Domain.Events.StoreEvents;

public record VehicleMaintenanceStartedStoreEvent(
    Guid VehicleId,
    Guid? PreviousDriverId,
    DateTime StartedAt);
