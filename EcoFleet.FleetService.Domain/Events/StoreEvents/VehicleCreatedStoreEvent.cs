namespace EcoFleet.FleetService.Domain.Events.StoreEvents;

public record VehicleCreatedStoreEvent(
    Guid VehicleId,
    string LicensePlate,
    double Latitude,
    double Longitude,
    DateTime CreatedAt);
