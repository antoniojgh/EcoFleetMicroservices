namespace EcoFleet.DriverService.Domain.Events.StoreEvents;

public record DriverEmailUpdatedStoreEvent(
    Guid DriverId,
    string Email,
    DateTime UpdatedAt);
