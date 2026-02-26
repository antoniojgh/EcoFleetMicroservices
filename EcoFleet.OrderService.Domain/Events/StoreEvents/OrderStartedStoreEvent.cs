namespace EcoFleet.OrderService.Domain.Events.StoreEvents;

public record OrderStartedStoreEvent(
    Guid OrderId,
    DateTime StartedAt);
