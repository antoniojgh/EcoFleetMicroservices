using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Aggregates;
using Marten;
using Microsoft.Extensions.Logging;

namespace EcoFleet.DriverService.Infrastructure.EventStore;

/// <summary>
/// Marten-based event store repository for the DriverAggregate.
/// Loads aggregate state by replaying events from PostgreSQL and appends
/// new uncommitted events to the stream on save.
/// </summary>
public class DriverEventStoreRepository : IDriverEventStore
{
    private readonly IDocumentSession _session;
    private readonly ILogger<DriverEventStoreRepository> _logger;

    public DriverEventStoreRepository(IDocumentSession session, ILogger<DriverEventStoreRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    /// <summary>
    /// Loads the DriverAggregate by replaying all events from its stream.
    /// Returns null if no event stream exists for the given driver ID.
    /// </summary>
    public async Task<DriverAggregate?> LoadAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading DriverAggregate from event store. DriverId: {DriverId}", driverId);

        var aggregate = await _session.Events.AggregateStreamAsync<DriverAggregate>(driverId, token: cancellationToken);

        if (aggregate is null)
        {
            _logger.LogDebug("No event stream found for driver {DriverId}.", driverId);
        }

        return aggregate;
    }

    /// <summary>
    /// Appends all uncommitted events from the DriverAggregate to its Marten event stream.
    /// Works for both new streams (first save after Create) and existing streams (subsequent saves).
    /// </summary>
    public async Task SaveAsync(DriverAggregate aggregate, CancellationToken cancellationToken = default)
    {
        if (!aggregate.UncommittedEvents.Any())
        {
            _logger.LogDebug("No uncommitted events for driver {DriverId}. Skipping save.", aggregate.Id);
            return;
        }

        _logger.LogDebug(
            "Appending {Count} event(s) to stream for driver {DriverId}.",
            aggregate.UncommittedEvents.Count,
            aggregate.Id);

        _session.Events.Append(aggregate.Id, aggregate.UncommittedEvents.ToArray());
        await _session.SaveChangesAsync(cancellationToken);

        aggregate.ClearUncommittedEvents();

        _logger.LogDebug("Events saved for driver {DriverId}.", aggregate.Id);
    }

    /// <summary>
    /// Archives the driver's event stream in Marten.
    /// The events are preserved for audit purposes but the stream is marked as deleted.
    /// The DriverReadModel projection is automatically removed by Marten on archive.
    /// </summary>
    public async Task DeleteAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Archiving event stream for driver {DriverId}.", driverId);

        _session.Events.ArchiveStream(driverId);
        await _session.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Event stream archived for driver {DriverId}.", driverId);
    }
}
