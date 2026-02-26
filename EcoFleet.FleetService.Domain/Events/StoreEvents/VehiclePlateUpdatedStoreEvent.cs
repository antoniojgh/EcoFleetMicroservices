namespace EcoFleet.FleetService.Domain.Events.StoreEvents;

public record VehiclePlateUpdatedStoreEvent(
    Guid VehicleId,
    string LicensePlate,
    DateTime UpdatedAt);
