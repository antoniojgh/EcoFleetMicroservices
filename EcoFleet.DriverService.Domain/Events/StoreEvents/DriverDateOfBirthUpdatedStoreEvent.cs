namespace EcoFleet.DriverService.Domain.Events.StoreEvents;

public record DriverDateOfBirthUpdatedStoreEvent(
    Guid DriverId,
    DateTime? DateOfBirth,
    DateTime UpdatedAt);
