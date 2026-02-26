using EcoFleet.DriverService.Domain.Aggregates;

namespace EcoFleet.DriverService.Application.Interfaces;

public interface IDriverEventStore
{
    Task<DriverAggregate?> LoadAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task SaveAsync(DriverAggregate aggregate, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid driverId, CancellationToken cancellationToken = default);
}
