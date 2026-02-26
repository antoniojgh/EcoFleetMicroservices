using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.ManagerService.Infrastructure.Persistence;

namespace EcoFleet.ManagerService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ManagerDbContext _dbContext;

    public UnitOfWork(ManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
