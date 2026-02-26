namespace EcoFleet.OrderService.Domain.Events.StoreEvents;

public record OrderCreatedStoreEvent(
    Guid OrderId,
    Guid DriverId,
    double PickUpLatitude,
    double PickUpLongitude,
    double DropOffLatitude,
    double DropOffLongitude,
    decimal Price,
    DateTime CreatedAt);
