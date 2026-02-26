namespace EcoFleet.OrderService.Domain.Events.StoreEvents;

public record OrderCancelledStoreEvent(
    Guid OrderId,
    string? CancellationReason,
    DateTime CancelledAt);
