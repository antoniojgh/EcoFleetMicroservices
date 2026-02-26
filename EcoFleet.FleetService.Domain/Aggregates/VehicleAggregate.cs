using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;
using EcoFleet.FleetService.Domain.Enums;
using EcoFleet.FleetService.Domain.Events.StoreEvents;

namespace EcoFleet.FleetService.Domain.Aggregates;

public class VehicleAggregate : EventSourcedAggregate
{
    public string LicensePlate { get; private set; } = string.Empty;
    public VehicleStatus Status { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public Guid? CurrentDriverId { get; private set; }

    // Parameterless constructor required by Marten for deserialization
    public VehicleAggregate() { }

    // --- FACTORY METHOD ---

    public static VehicleAggregate Create(
        string licensePlate,
        double latitude,
        double longitude)
    {
        var aggregate = new VehicleAggregate();
        aggregate.RaiseEvent(new VehicleCreatedStoreEvent(
            Guid.NewGuid(),
            licensePlate,
            latitude,
            longitude,
            DateTime.UtcNow));
        return aggregate;
    }

    // --- BEHAVIORS ---

    public void AssignDriver(Guid driverId)
    {
        if (Status == VehicleStatus.Maintenance)
            throw new DomainException("Cannot assign a driver to a vehicle in maintenance.");

        RaiseEvent(new VehicleDriverAssignedStoreEvent(Id, driverId, DateTime.UtcNow));
    }

    public void UnassignDriver()
    {
        if (CurrentDriverId is null)
            return;

        if (Status == VehicleStatus.Maintenance)
            return;

        RaiseEvent(new VehicleDriverUnassignedStoreEvent(Id, DateTime.UtcNow));
    }

    public void MarkForMaintenance()
    {
        if (Status == VehicleStatus.Active)
            throw new DomainException("Cannot maintain a vehicle currently in use.");

        RaiseEvent(new VehicleMaintenanceStartedStoreEvent(Id, CurrentDriverId, DateTime.UtcNow));
    }

    public void UpdatePlate(string licensePlate)
    {
        RaiseEvent(new VehiclePlateUpdatedStoreEvent(Id, licensePlate, DateTime.UtcNow));
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        RaiseEvent(new VehicleLocationUpdatedStoreEvent(Id, latitude, longitude, DateTime.UtcNow));
    }

    // --- STATE RECONSTRUCTION ---

    // Marten calls Apply() to rebuild the aggregate state by replaying the event stream
    public override void Apply(object @event)
    {
        switch (@event)
        {
            case VehicleCreatedStoreEvent e:
                Id = e.VehicleId;
                LicensePlate = e.LicensePlate;
                Latitude = e.Latitude;
                Longitude = e.Longitude;
                Status = VehicleStatus.Idle;
                break;

            case VehicleDriverAssignedStoreEvent e:
                CurrentDriverId = e.DriverId;
                Status = VehicleStatus.Active;
                break;

            case VehicleDriverUnassignedStoreEvent:
                CurrentDriverId = null;
                Status = VehicleStatus.Idle;
                break;

            case VehicleMaintenanceStartedStoreEvent:
                Status = VehicleStatus.Maintenance;
                CurrentDriverId = null;
                break;

            case VehiclePlateUpdatedStoreEvent e:
                LicensePlate = e.LicensePlate;
                break;

            case VehicleLocationUpdatedStoreEvent e:
                Latitude = e.Latitude;
                Longitude = e.Longitude;
                break;
        }
    }
}
