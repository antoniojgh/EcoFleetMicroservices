namespace EcoFleet.BuildingBlocks.Domain;

public abstract class EventSourcedAggregate
{
    public Guid Id { get; protected set; }
    public int Version { get; set; }

    private readonly List<object> _uncommittedEvents = new();
    public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents;

    protected void RaiseEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
        Apply(@event);
    }

    // Each aggregate implements this to rebuild state from events
    public abstract void Apply(object @event);

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();
}