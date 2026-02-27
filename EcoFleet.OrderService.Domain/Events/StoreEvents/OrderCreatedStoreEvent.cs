namespace EcoFleet.OrderService.Domain.Events.StoreEvents;

public record OrderCreatedStoreEvent(
    Guid OrderId,
    Guid DriverId,
    string DriverFirstName,
    string DriverLastName,
    string DriverEmail,
    double PickUpLatitude,
    double PickUpLongitude,
    double DropOffLatitude,
    double DropOffLongitude,
    decimal Price,
    DateTime CreatedAt);
