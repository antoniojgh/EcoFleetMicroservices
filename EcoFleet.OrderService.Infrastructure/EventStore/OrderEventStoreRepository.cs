using EcoFleet.OrderService.Application.Interfaces;
using EcoFleet.OrderService.Domain.Aggregates;
using Marten;
using Microsoft.Extensions.Logging;

namespace EcoFleet.OrderService.Infrastructure.EventStore;

/// <summary>
/// Marten-based event store repository for the OrderAggregate.
/// Loads aggregate state by replaying events from PostgreSQL and appends
/// new uncommitted events to the stream on save.
/// </summary>
public class OrderEventStoreRepository : IOrderEventStore
{
    private readonly IDocumentSession _session;
    private readonly ILogger<OrderEventStoreRepository> _logger;

    public OrderEventStoreRepository(IDocumentSession session, ILogger<OrderEventStoreRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    /// <summary>
    /// Loads the OrderAggregate by replaying all events from its stream.
    /// Returns null if no event stream exists for the given order ID.
    /// </summary>
    public async Task<OrderAggregate?> LoadAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading OrderAggregate from event store. OrderId: {OrderId}", orderId);

        var aggregate = await _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: cancellationToken);

        if (aggregate is null)
        {
            _logger.LogDebug("No event stream found for order {OrderId}.", orderId);
        }

        return aggregate;
    }

    /// <summary>
    /// Appends all uncommitted events from the OrderAggregate to its Marten event stream.
    /// Works for both new streams (first save after Create) and existing streams (subsequent saves).
    /// </summary>
    public async Task SaveAsync(OrderAggregate aggregate, CancellationToken cancellationToken = default)
    {
        if (!aggregate.UncommittedEvents.Any())
        {
            _logger.LogDebug("No uncommitted events for order {OrderId}. Skipping save.", aggregate.Id);
            return;
        }

        _logger.LogDebug(
            "Appending {Count} event(s) to stream for order {OrderId}.",
            aggregate.UncommittedEvents.Count,
            aggregate.Id);

        _session.Events.Append(aggregate.Id, aggregate.UncommittedEvents.ToArray());
        await _session.SaveChangesAsync(cancellationToken);

        aggregate.ClearUncommittedEvents();
    }

    /// <summary>
    /// Archives the event stream for the given order ID.
    /// Events are preserved for audit purposes, but the read model is removed.
    /// </summary>
    public async Task DeleteAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Archiving event stream for order {OrderId}.", orderId);

        _session.Events.ArchiveStream(orderId);
        await _session.SaveChangesAsync(cancellationToken);
    }
}
