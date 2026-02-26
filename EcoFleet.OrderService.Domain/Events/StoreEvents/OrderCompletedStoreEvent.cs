namespace EcoFleet.OrderService.Domain.Events.StoreEvents;

public record OrderCompletedStoreEvent(
    Guid OrderId,
    DateTime CompletedAt);
