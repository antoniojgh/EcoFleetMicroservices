using EcoFleet.ManagerService.Application.DTOs;
using EcoFleet.ManagerService.Application.Interfaces;
using EcoFleet.ManagerService.Domain.Entities;
using EcoFleet.ManagerService.Domain.ValueObjects;
using EcoFleet.ManagerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.ManagerService.Infrastructure.Repositories;

public class ManagerRepository : IManagerRepository
{
    private readonly ManagerDbContext _context;

    public ManagerRepository(ManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Manager?> GetByIdAsync(ManagerId id, CancellationToken cancellationToken = default)
    {
        return await _context.Managers.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Manager>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Managers.ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalNumberOfRecords(CancellationToken cancellationToken = default)
    {
        return await _context.Managers.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Manager entity, CancellationToken cancellationToken = default)
    {
        await _context.Managers.AddAsync(entity, cancellationToken);
    }

    public Task Update(Manager entity, CancellationToken cancellationToken = default)
    {
        _context.Managers.Update(entity);
        return Task.CompletedTask;
    }

    public Task Delete(Manager entity, CancellationToken cancellationToken = default)
    {
        _context.Managers.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Manager>> GetFilteredAsync(FilterManagerDTO filterManagerDTO, CancellationToken cancellationToken = default)
    {
        var queryable = BuildFilterQuery(filterManagerDTO);

        return await queryable
            .OrderBy(x => x.Id)
            .Skip((filterManagerDTO.Page - 1) * filterManagerDTO.RecordsByPage)
            .Take(filterManagerDTO.RecordsByPage)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFilteredCountAsync(FilterManagerDTO filterManagerDTO, CancellationToken cancellationToken = default)
    {
        return await BuildFilterQuery(filterManagerDTO).CountAsync(cancellationToken);
    }

    private IQueryable<Manager> BuildFilterQuery(FilterManagerDTO filterManagerDTO)
    {
        var queryable = _context.Managers.AsQueryable();

        if (filterManagerDTO.Id is not null)
        {
            var managerId = new ManagerId(filterManagerDTO.Id.Value);
            queryable = queryable.Where(x => x.Id == managerId);
        }

        if (filterManagerDTO.FirstName is not null)
            queryable = queryable.Where(x => x.Name.FirstName.Contains(filterManagerDTO.FirstName));

        if (filterManagerDTO.LastName is not null)
            queryable = queryable.Where(x => x.Name.LastName.Contains(filterManagerDTO.LastName));

        if (filterManagerDTO.Email is not null)
        {
            var email = Email.TryCreate(filterManagerDTO.Email);
            if (email is not null)
                queryable = queryable.Where(x => x.Email == email);
        }

        return queryable;
    }
}
