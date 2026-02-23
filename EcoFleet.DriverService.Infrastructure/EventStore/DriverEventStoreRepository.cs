using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using Marten;
using Microsoft.Extensions.Logging;

namespace EcoFleet.DriverService.Infrastructure.EventStore;

/// <summary>
/// Marten-based event store repository for the Driver aggregate.
/// Stores driver state changes as a sequence of events in PostgreSQL via Marten,
/// enabling event sourcing and full audit trails.
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
    /// Loads the Driver aggregate by replaying all events from its stream.
    /// Returns null if no events exist for the given driver ID.
    /// </summary>
    public async Task<Driver?> LoadAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading driver aggregate from event store. DriverId: {DriverId}", driverId);

        var driver = await _session.Events.AggregateStreamAsync<Driver>(driverId, token: cancellationToken);

        if (driver is null)
        {
            _logger.LogDebug("No event stream found for driver {DriverId}.", driverId);
        }

        return driver;
    }

    /// <summary>
    /// Persists uncommitted domain events for the Driver aggregate to the Marten event store.
    /// Events are appended to the driver's event stream and cleared after saving.
    /// </summary>
    public async Task SaveAsync(Driver driver, CancellationToken cancellationToken = default)
    {
        if (!driver.DomainEvents.Any())
        {
            _logger.LogDebug("No uncommitted events for driver {DriverId}. Skipping save.", driver.Id.Value);
            return;
        }

        _logger.LogDebug(
            "Appending {Count} event(s) to stream for driver {DriverId}.",
            driver.DomainEvents.Count,
            driver.Id.Value);

        _session.Events.Append(driver.Id.Value, driver.DomainEvents.ToArray());
        await _session.SaveChangesAsync(cancellationToken);

        driver.ClearDomainEvents();

        _logger.LogDebug("Events saved for driver {DriverId}.", driver.Id.Value);
    }
}
