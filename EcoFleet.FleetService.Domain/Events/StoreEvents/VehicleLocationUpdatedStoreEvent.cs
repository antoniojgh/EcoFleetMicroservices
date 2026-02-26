namespace EcoFleet.FleetService.Domain.Events.StoreEvents;

public record VehicleLocationUpdatedStoreEvent(
    Guid VehicleId,
    double Latitude,
    double Longitude,
    DateTime UpdatedAt);
