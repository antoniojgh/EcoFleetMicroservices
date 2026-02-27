using System.Text.Json;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.BuildingBlocks.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;

        public UnitOfWork(DbContext dbContext)
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
            // Check if some entity that implement IHasDomainEvents has been modified and has new events created
            // retrieved all entities with that condition
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

            _dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        }
    }
}
