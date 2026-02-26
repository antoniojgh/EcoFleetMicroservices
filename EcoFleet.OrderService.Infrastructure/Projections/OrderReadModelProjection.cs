using EcoFleet.OrderService.Domain.Enums;
using EcoFleet.OrderService.Domain.Events.StoreEvents;
using Marten.Events.Aggregation;

namespace EcoFleet.OrderService.Infrastructure.Projections;

public class OrderReadModelProjection : SingleStreamProjection<OrderReadModel, Guid>
{
    public OrderReadModel Create(OrderCreatedStoreEvent @event) => new()
    {
        Id = @event.OrderId,
        DriverId = @event.DriverId,
        PickUpLatitude = @event.PickUpLatitude,
        PickUpLongitude = @event.PickUpLongitude,
        DropOffLatitude = @event.DropOffLatitude,
        DropOffLongitude = @event.DropOffLongitude,
        Price = @event.Price,
        Status = OrderStatus.Pending.ToString(),
        CreatedDate = @event.CreatedAt
    };

    public void Apply(OrderStartedStoreEvent @event, OrderReadModel model)
    {
        model.Status = OrderStatus.InProgress.ToString();
        model.UpdatedAt = @event.StartedAt;
    }

    public void Apply(OrderCompletedStoreEvent @event, OrderReadModel model)
    {
        model.Status = OrderStatus.Completed.ToString();
        model.UpdatedAt = @event.CompletedAt;
    }

    public void Apply(OrderCancelledStoreEvent @event, OrderReadModel model)
    {
        model.Status = OrderStatus.Cancelled.ToString();
        model.CancellationReason = @event.CancellationReason;
        model.UpdatedAt = @event.CancelledAt;
    }
}
