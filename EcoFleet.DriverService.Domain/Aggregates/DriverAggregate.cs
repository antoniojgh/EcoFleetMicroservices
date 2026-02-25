using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;
using EcoFleet.DriverService.Domain.Enums;
using EcoFleet.DriverService.Domain.Events.StoreEvents;

namespace EcoFleet.DriverService.Domain.Aggregates;

public class DriverAggregate : EventSourcedAggregate
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string License { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public DriverStatus Status { get; private set; }
    public Guid? AssignedVehicleId { get; private set; }

    // Parameterless constructor required by Marten for deserialization
    public DriverAggregate() { }

    // --- FACTORY METHOD ---

    public static DriverAggregate Create(
        string firstName,
        string lastName,
        string license,
        string email,
        string? phoneNumber,
        DateTime? dateOfBirth)
    {
        var aggregate = new DriverAggregate();
        aggregate.RaiseEvent(new DriverCreatedStoreEvent(
            Guid.NewGuid(),
            firstName,
            lastName,
            license,
            email,
            phoneNumber,
            dateOfBirth,
            DateTime.UtcNow));
        return aggregate;
    }

    // --- BEHAVIORS ---

    public void UpdateName(string firstName, string lastName)
    {
        RaiseEvent(new DriverNameUpdatedStoreEvent(Id, firstName, lastName, DateTime.UtcNow));
    }

    public void AssignVehicle(Guid vehicleId)
    {
        if (Status == DriverStatus.Suspended)
            throw new DomainException("Cannot assign a vehicle to a suspended driver.");

        RaiseEvent(new DriverVehicleAssignedStoreEvent(Id, vehicleId, DateTime.UtcNow));
    }

    public void UnassignVehicle()
    {
        if (AssignedVehicleId is null)
            return;

        RaiseEvent(new DriverVehicleUnassignedStoreEvent(Id, DateTime.UtcNow));
    }

    public void Suspend()
    {
        if (Status == DriverStatus.OnDuty)
            throw new DomainException("Cannot suspend a driver currently on duty.");

        RaiseEvent(new DriverSuspendedStoreEvent(Id, DateTime.UtcNow));
    }

    public void Reinstate()
    {
        if (Status != DriverStatus.Suspended)
            throw new DomainException("Only suspended drivers can be reinstated.");

        RaiseEvent(new DriverReinstatedStoreEvent(Id, DateTime.UtcNow));
    }

    // --- STATE RECONSTRUCTION ---

    // Marten calls Apply() to rebuild the aggregate state by replaying the event stream
    public override void Apply(object @event)
    {
        switch (@event)
        {
            case DriverCreatedStoreEvent e:
                Id = e.DriverId;
                FirstName = e.FirstName;
                LastName = e.LastName;
                License = e.License;
                Email = e.Email;
                PhoneNumber = e.PhoneNumber;
                DateOfBirth = e.DateOfBirth;
                Status = DriverStatus.Available;
                break;

            case DriverNameUpdatedStoreEvent e:
                FirstName = e.FirstName;
                LastName = e.LastName;
                break;

            case DriverVehicleAssignedStoreEvent e:
                AssignedVehicleId = e.VehicleId;
                Status = DriverStatus.OnDuty;
                break;

            case DriverVehicleUnassignedStoreEvent:
                AssignedVehicleId = null;
                Status = DriverStatus.Available;
                break;

            case DriverSuspendedStoreEvent:
                Status = DriverStatus.Suspended;
                AssignedVehicleId = null;
                break;

            case DriverReinstatedStoreEvent:
                Status = DriverStatus.Available;
                break;
        }
    }
}