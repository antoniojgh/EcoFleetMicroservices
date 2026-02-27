using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;
using EcoFleet.OrderService.Domain.Enums;
using EcoFleet.OrderService.Domain.Events.StoreEvents;

namespace EcoFleet.OrderService.Domain.Aggregates;

public class OrderAggregate : EventSourcedAggregate
{
    public Guid DriverId { get; private set; }
    public string DriverFirstName { get; private set; } = string.Empty;
    public string DriverLastName { get; private set; } = string.Empty;
    public string DriverEmail { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public double PickUpLatitude { get; private set; }
    public double PickUpLongitude { get; private set; }
    public double DropOffLatitude { get; private set; }
    public double DropOffLongitude { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public string? CancellationReason { get; private set; }

    // Parameterless constructor required by Marten for deserialization
    public OrderAggregate() { }

    // --- FACTORY METHOD ---

    public static OrderAggregate Create(
        Guid driverId,
        string driverFirstName,
        string driverLastName,
        string driverEmail,
        double pickUpLatitude,
        double pickUpLongitude,
        double dropOffLatitude,
        double dropOffLongitude,
        decimal price)
    {
        var aggregate = new OrderAggregate();
        aggregate.RaiseEvent(new OrderCreatedStoreEvent(
            Guid.NewGuid(),
            driverId,
            driverFirstName,
            driverLastName,
            driverEmail,
            pickUpLatitude,
            pickUpLongitude,
            dropOffLatitude,
            dropOffLongitude,
            price,
            DateTime.UtcNow));
        return aggregate;
    }

    // --- BEHAVIORS ---

    public void Start()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be started.");

        RaiseEvent(new OrderStartedStoreEvent(Id, DateTime.UtcNow));
    }

    public void Complete()
    {
        if (Status != OrderStatus.InProgress)
            throw new DomainException("Only in-progress orders can be completed.");

        RaiseEvent(new OrderCompletedStoreEvent(Id, DateTime.UtcNow));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Completed)
            throw new DomainException("Cannot cancel a completed order.");

        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Order is already cancelled.");

        RaiseEvent(new OrderCancelledStoreEvent(Id, reason, DateTime.UtcNow));
    }

    // --- STATE RECONSTRUCTION ---

    // Marten calls Apply() to rebuild the aggregate state by replaying the event stream
    public override void Apply(object @event)
    {
        switch (@event)
        {
            case OrderCreatedStoreEvent e:
                Id = e.OrderId;
                DriverId = e.DriverId;
                DriverFirstName = e.DriverFirstName;
                DriverLastName = e.DriverLastName;
                DriverEmail = e.DriverEmail;
                PickUpLatitude = e.PickUpLatitude;
                PickUpLongitude = e.PickUpLongitude;
                DropOffLatitude = e.DropOffLatitude;
                DropOffLongitude = e.DropOffLongitude;
                Price = e.Price;
                Status = OrderStatus.Pending;
                CreatedDate = e.CreatedAt;
                break;

            case OrderStartedStoreEvent:
                Status = OrderStatus.InProgress;
                break;

            case OrderCompletedStoreEvent:
                Status = OrderStatus.Completed;
                break;

            case OrderCancelledStoreEvent e:
                Status = OrderStatus.Cancelled;
                CancellationReason = e.CancellationReason;
                break;
        }
    }
}
