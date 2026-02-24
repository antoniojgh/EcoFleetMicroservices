using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.FleetService.Domain.Entities;

namespace EcoFleet.FleetService.Application.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle, VehicleId>
{
    /// <summary>
    /// Returns all vehicles currently assigned to the specified driver.
    /// Used by the DriverSuspendedConsumer to unassign vehicles when a driver is suspended.
    /// </summary>
    Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
}
