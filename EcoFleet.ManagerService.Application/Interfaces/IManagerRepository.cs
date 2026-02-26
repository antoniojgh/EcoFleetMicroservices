using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.ManagerService.Application.DTOs;
using EcoFleet.ManagerService.Domain.Entities;

namespace EcoFleet.ManagerService.Application.Interfaces;

public interface IManagerRepository : IRepository<Manager, ManagerId>
{
    Task<IEnumerable<Manager>> GetFilteredAsync(FilterManagerDTO filterManagerDTO, CancellationToken cancellationToken = default);
}
