namespace EcoFleet.DriverService.Domain.Events.StoreEvents;

public record DriverLicenseUpdatedStoreEvent(
    Guid DriverId,
    string License,
    DateTime UpdatedAt);
