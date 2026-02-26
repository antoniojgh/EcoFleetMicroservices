namespace EcoFleet.FleetService.Domain.Events.StoreEvents;

public record VehicleDriverUnassignedStoreEvent(
    Guid VehicleId,
    DateTime UnassignedAt);
