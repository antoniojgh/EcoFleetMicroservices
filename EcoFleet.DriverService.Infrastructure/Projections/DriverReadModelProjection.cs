using EcoFleet.DriverService.Domain.Enums;
using EcoFleet.DriverService.Domain.Events.StoreEvents;
using Marten.Events.Aggregation;

namespace EcoFleet.DriverService.Infrastructure.Projections
{
    public class DriverReadModelProjection : SingleStreamProjection<DriverReadModel>
    {
        public DriverReadModel Create(DriverCreatedStoreEvent @event) => new()
        {
            Id = @event.DriverId,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            License = @event.License,
            Email = @event.Email,
            PhoneNumber = @event.PhoneNumber,
            DateOfBirth = @event.DateOfBirth,
            Status = DriverStatus.Available.ToString(),
            CreatedAt = @event.CreatedAt
        };

        public void Apply(DriverNameUpdatedStoreEvent @event, DriverReadModel model)
        {
            model.FirstName = @event.FirstName;
            model.LastName = @event.LastName;
            model.UpdatedAt = @event.UpdatedAt;
        }

        public void Apply(DriverSuspendedStoreEvent @event, DriverReadModel model)
        {
            model.Status = DriverStatus.Suspended.ToString();
            model.AssignedVehicleId = null;
            model.UpdatedAt = @event.SuspendedAt;
        }

        public void Apply(DriverReinstatedStoreEvent @event, DriverReadModel model)
        {
            model.Status = DriverStatus.Available.ToString();
            model.UpdatedAt = @event.ReinstatedAt;
        }

        public void Apply(DriverVehicleAssignedStoreEvent @event, DriverReadModel model)
        {
            model.AssignedVehicleId = @event.VehicleId;
            model.Status = DriverStatus.OnDuty.ToString();
            model.UpdatedAt = @event.AssignedAt;
        }

        public void Apply(DriverVehicleUnassignedStoreEvent @event, DriverReadModel model)
        {
            model.AssignedVehicleId = null;
            model.Status = DriverStatus.Available.ToString();
            model.UpdatedAt = @event.UnassignedAt;
        }
    }
}