using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;
using EcoFleet.FleetService.Domain.Enums;
using EcoFleet.FleetService.Domain.Events;
using EcoFleet.FleetService.Domain.ValueObjects;

namespace EcoFleet.FleetService.Domain.Entities;

public class Vehicle : Entity<VehicleId>, IAggregateRoot
{
    public LicensePlate Plate { get; private set; }
    public VehicleStatus Status { get; private set; }
    public Geolocation CurrentLocation { get; private set; }

    // In microservices, we use a primitive Guid instead of a strongly-typed DriverId
    // to avoid cross-boundary coupling with the DriverService
    public Guid? CurrentDriverId { get; private set; }

    // Constructor for EF Core
    private Vehicle() : base(new VehicleId(Guid.NewGuid()))
    { }

    // Public Constructor: vehicle without a driver
    public Vehicle(LicensePlate plate, Geolocation initialLocation) : base(new VehicleId(Guid.NewGuid()))
    {
        Plate = plate;
        CurrentLocation = initialLocation;
        Status = VehicleStatus.Idle;
    }

    // Public Constructor: vehicle with an initial driver assignment
    public Vehicle(LicensePlate plate, Geolocation initialLocation, Guid initialDriverId) : base(new VehicleId(Guid.NewGuid()))
    {
        Plate = plate;
        CurrentLocation = initialLocation;
        CurrentDriverId = initialDriverId;
        Status = VehicleStatus.Active;

        AddDomainEvent(new VehicleDriverAssignedEvent(Id, initialDriverId, DateTime.UtcNow));
    }

    // --- BEHAVIORS ---

    public void UpdateTelemetry(Geolocation newLocation)
    {
        CurrentLocation = newLocation;
    }

    public void AssignDriver(Guid driverId)
    {
        if (Status == VehicleStatus.Maintenance)
            throw new DomainException("Cannot assign a driver to a vehicle in maintenance.");

        CurrentDriverId = driverId;
        Status = VehicleStatus.Active;

        AddDomainEvent(new VehicleDriverAssignedEvent(Id, driverId, DateTime.UtcNow));
    }

    public void MarkForMaintenance()
    {
        if (Status == VehicleStatus.Active)
            throw new DomainException("Cannot maintain a vehicle currently in use.");

        Status = VehicleStatus.Maintenance;
        CurrentDriverId = null;

        AddDomainEvent(new VehicleMaintenanceStartedEvent(Id, DateTime.UtcNow));
    }

    public void UpdatePlate(LicensePlate plate)
    {
        Plate = plate;
    }

    public void UnassignDriver()
    {
        if (Status == VehicleStatus.Maintenance)
            return;

        CurrentDriverId = null;
        Status = VehicleStatus.Idle;

        AddDomainEvent(new VehicleDriverUnassignedEvent(Id, DateTime.UtcNow));
    }
}
