using EcoFleet.OrderService.Domain.Aggregates;

namespace EcoFleet.OrderService.Application.Interfaces;

public interface IOrderEventStore
{
    Task<OrderAggregate?> LoadAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task SaveAsync(OrderAggregate aggregate, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid orderId, CancellationToken cancellationToken = default);
}
