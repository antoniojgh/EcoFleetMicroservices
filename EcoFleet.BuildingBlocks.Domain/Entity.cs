namespace EcoFleet.BuildingBlocks.Domain
{
    public abstract class Entity<TId> : IEquatable<Entity<TId>>, IHasDomainEvents
    {
        public TId Id { get; protected set; }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected Entity(TId id) => Id = id;

        // Use this to add events (e.g., "TruckArrived")
        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents() => _domainEvents.Clear();

        public override bool Equals(object? obj) => obj is Entity<TId> other && EqualityComparer<TId>.Default.Equals(Id, other.Id);
        public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id!);
        public bool Equals(Entity<TId>? other) => Equals((object?)other);
    }
}




