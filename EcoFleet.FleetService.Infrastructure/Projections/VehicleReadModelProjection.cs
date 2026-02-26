using EcoFleet.FleetService.Domain.Enums;
using EcoFleet.FleetService.Domain.Events.StoreEvents;
using Marten.Events.Aggregation;

namespace EcoFleet.FleetService.Infrastructure.Projections;

public class VehicleReadModelProjection : SingleStreamProjection<VehicleReadModel, Guid>
{
    public VehicleReadModel Create(VehicleCreatedStoreEvent @event) => new()
    {
        Id = @event.VehicleId,
        LicensePlate = @event.LicensePlate,
        Latitude = @event.Latitude,
        Longitude = @event.Longitude,
        Status = VehicleStatus.Idle.ToString(),
        CreatedAt = @event.CreatedAt
    };

    public void Apply(VehicleDriverAssignedStoreEvent @event, VehicleReadModel model)
    {
        model.CurrentDriverId = @event.DriverId;
        model.Status = VehicleStatus.Active.ToString();
        model.UpdatedAt = @event.AssignedAt;
    }

    public void Apply(VehicleDriverUnassignedStoreEvent @event, VehicleReadModel model)
    {
        model.CurrentDriverId = null;
        model.Status = VehicleStatus.Idle.ToString();
        model.UpdatedAt = @event.UnassignedAt;
    }

    public void Apply(VehicleMaintenanceStartedStoreEvent @event, VehicleReadModel model)
    {
        model.Status = VehicleStatus.Maintenance.ToString();
        model.CurrentDriverId = null;
        model.UpdatedAt = @event.StartedAt;
    }

    public void Apply(VehiclePlateUpdatedStoreEvent @event, VehicleReadModel model)
    {
        model.LicensePlate = @event.LicensePlate;
        model.UpdatedAt = @event.UpdatedAt;
    }

    public void Apply(VehicleLocationUpdatedStoreEvent @event, VehicleReadModel model)
    {
        model.Latitude = @event.Latitude;
        model.Longitude = @event.Longitude;
        model.UpdatedAt = @event.UpdatedAt;
    }
}
