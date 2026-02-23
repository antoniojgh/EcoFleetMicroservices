using EcoFleet.DriverService.Domain.Entities;

namespace EcoFleet.DriverService.Application.Interfaces
{
    public interface IDriverEventStore
    {
        Task<Driver?> LoadAsync(Guid driverId, CancellationToken cancellationToken = default);
        Task SaveAsync(Driver driver, CancellationToken cancellationToken = default);
    }
}
