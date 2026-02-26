namespace EcoFleet.DriverService.Domain.Events.StoreEvents;

public record DriverPhoneNumberUpdatedStoreEvent(
    Guid DriverId,
    string? PhoneNumber,
    DateTime UpdatedAt);
