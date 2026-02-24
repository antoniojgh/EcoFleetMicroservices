using System.Text.Json;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.FleetService.Infrastructure.Outbox;
using EcoFleet.FleetService.Infrastructure.Persistence;

namespace EcoFleet.FleetService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FleetDbContext _dbContext;

    public UnitOfWork(FleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Convert Domain Events into OutboxMessages (same transaction)
        ConvertDomainEventsToOutboxMessages();

        // 2. Commit everything atomically (entity changes + outbox messages)
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void ConvertDomainEventsToOutboxMessages()
    {
        var domainEntities = _dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        if (!domainEntities.Any())
            return;

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        var outboxMessages = domainEvents.Select(domainEvent => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = domainEvent.GetType().AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            OccurredOn = domainEvent.OcurredOn,
            ProcessedOn = null,
            Error = null
        });

        _dbContext.OutboxMessages.AddRange(outboxMessages);
    }
}
