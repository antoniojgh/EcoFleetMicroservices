using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Aggregates;
using Marten;
using Microsoft.Extensions.Logging;

namespace EcoFleet.FleetService.Infrastructure.EventStore;

/// <summary>
/// Marten-based event store repository for the VehicleAggregate.
/// Loads aggregate state by replaying events from PostgreSQL and appends
/// new uncommitted events to the stream on save.
/// </summary>
public class VehicleEventStoreRepository : IVehicleEventStore
{
    private readonly IDocumentSession _session;
    private readonly ILogger<VehicleEventStoreRepository> _logger;

    public VehicleEventStoreRepository(IDocumentSession session, ILogger<VehicleEventStoreRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    /// <summary>
    /// Loads the VehicleAggregate by replaying all events from its stream.
    /// Returns null if no event stream exists for the given vehicle ID.
    /// </summary>
    public async Task<VehicleAggregate?> LoadAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading VehicleAggregate from event store. VehicleId: {VehicleId}", vehicleId);

        var aggregate = await _session.Events.AggregateStreamAsync<VehicleAggregate>(vehicleId, token: cancellationToken);

        if (aggregate is null)
        {
            _logger.LogDebug("No event stream found for vehicle {VehicleId}.", vehicleId);
        }

        return aggregate;
    }

    /// <summary>
    /// Appends all uncommitted events from the VehicleAggregate to its Marten event stream.
    /// Works for both new streams (first save after Create) and existing streams (subsequent saves).
    /// </summary>
    public async Task SaveAsync(VehicleAggregate aggregate, CancellationToken cancellationToken = default)
    {
        if (!aggregate.UncommittedEvents.Any())
        {
            _logger.LogDebug("No uncommitted events for vehicle {VehicleId}. Skipping save.", aggregate.Id);
            return;
        }

        _logger.LogDebug(
            "Appending {Count} event(s) to stream for vehicle {VehicleId}.",
            aggregate.UncommittedEvents.Count,
            aggregate.Id);

        _session.Events.Append(aggregate.Id, aggregate.UncommittedEvents.ToArray());
        await _session.SaveChangesAsync(cancellationToken);

        aggregate.ClearUncommittedEvents();
    }

    /// <summary>
    /// Archives the event stream for the given vehicle ID.
    /// Events are preserved for audit purposes, but the read model is removed.
    /// </summary>
    public async Task DeleteAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Archiving event stream for vehicle {VehicleId}.", vehicleId);

        _session.Events.ArchiveStream(vehicleId);
        await _session.SaveChangesAsync(cancellationToken);
    }
}
