using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.DriverService.Application.DTOs;
using EcoFleet.DriverService.Domain.Entities;

namespace EcoFleet.DriverService.Application.Interfaces;

public interface IDriverRepository : IRepository<Driver, DriverId>
{
    Task<IEnumerable<Driver>> GetFilteredAsync(FilterDriverDTO filterDriverDTO, CancellationToken cancellationToken = default);
}
