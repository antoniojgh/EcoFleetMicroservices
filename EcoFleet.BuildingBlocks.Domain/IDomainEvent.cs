using MediatR;

namespace EcoFleet.BuildingBlocks.Domain
{
    // Represents something important that happened in the business.
    public interface IDomainEvent : INotification
    {
        DateTime OcurredOn { get; }
    }
}
