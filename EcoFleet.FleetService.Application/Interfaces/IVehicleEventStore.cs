using EcoFleet.FleetService.Domain.Aggregates;

namespace EcoFleet.FleetService.Application.Interfaces;

public interface IVehicleEventStore
{
    Task<VehicleAggregate?> LoadAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task SaveAsync(VehicleAggregate aggregate, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
