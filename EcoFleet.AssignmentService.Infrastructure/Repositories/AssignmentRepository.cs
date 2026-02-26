using EcoFleet.AssignmentService.Application.DTOs;
using EcoFleet.AssignmentService.Application.Interfaces;
using EcoFleet.AssignmentService.Domain.Entities;
using EcoFleet.AssignmentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.AssignmentService.Infrastructure.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AssignmentDbContext _context;

    public AssignmentRepository(AssignmentDbContext context)
    {
        _context = context;
    }

    public async Task<ManagerDriverAssignment?> GetByIdAsync(ManagerDriverAssignmentId id, CancellationToken cancellationToken = default)
    {
        return await _context.Assignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ManagerDriverAssignment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Assignments.ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalNumberOfRecords(CancellationToken cancellationToken = default)
    {
        return await _context.Assignments.CountAsync(cancellationToken);
    }

    public async Task AddAsync(ManagerDriverAssignment entity, CancellationToken cancellationToken = default)
    {
        await _context.Assignments.AddAsync(entity, cancellationToken);
    }

    public Task Update(ManagerDriverAssignment entity, CancellationToken cancellationToken = default)
    {
        _context.Assignments.Update(entity);
        return Task.CompletedTask;
    }

    public Task Delete(ManagerDriverAssignment entity, CancellationToken cancellationToken = default)
    {
        _context.Assignments.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ManagerDriverAssignment>> GetActiveByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        return await _context.Assignments
            .Where(a => a.DriverId == driverId && a.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ManagerDriverAssignment>> GetFilteredAsync(FilterAssignmentDTO filter, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Assignments.AsQueryable();

        if (filter.Id is not null)
        {
            var assignmentId = new ManagerDriverAssignmentId(filter.Id.Value);
            queryable = queryable.Where(x => x.Id == assignmentId);
        }

        if (filter.ManagerId is not null)
        {
            queryable = queryable.Where(x => x.ManagerId == filter.ManagerId.Value);
        }

        if (filter.DriverId is not null)
        {
            queryable = queryable.Where(x => x.DriverId == filter.DriverId.Value);
        }

        if (filter.IsActive is not null)
        {
            queryable = queryable.Where(x => x.IsActive == filter.IsActive.Value);
        }

        return await queryable
            .OrderBy(x => x.Id)
            .Skip((filter.Page - 1) * filter.RecordsByPage)
            .Take(filter.RecordsByPage)
            .ToListAsync(cancellationToken);
    }
}
