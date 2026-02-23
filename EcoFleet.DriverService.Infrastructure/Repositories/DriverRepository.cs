using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.DriverService.Application.DTOs;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using EcoFleet.DriverService.Domain.ValueObjects;
using EcoFleet.DriverService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.DriverService.Infrastructure.Repositories;

public class DriverRepository : IDriverRepository
{
    private readonly DriverDbContext _context;

    public DriverRepository(DriverDbContext context)
    {
        _context = context;
    }

    public async Task<Driver?> GetByIdAsync(DriverId id, CancellationToken cancellationToken = default)
    {
        return await _context.Drivers.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Driver>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Drivers.ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalNumberOfRecords(CancellationToken cancellationToken = default)
    {
        return await _context.Drivers.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Driver entity, CancellationToken cancellationToken = default)
    {
        await _context.Drivers.AddAsync(entity, cancellationToken);
    }

    public Task Update(Driver entity, CancellationToken cancellationToken = default)
    {
        _context.Drivers.Update(entity);
        return Task.CompletedTask;
    }

    public Task Delete(Driver entity, CancellationToken cancellationToken = default)
    {
        _context.Drivers.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Driver>> GetFilteredAsync(FilterDriverDTO filterDriverDTO, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Drivers.AsQueryable();

        if (filterDriverDTO.Id is not null)
        {
            var driverId = new DriverId(filterDriverDTO.Id.Value);
            queryable = queryable.Where(x => x.Id == driverId);
        }

        if (filterDriverDTO.FirstName is not null)
        {
            queryable = queryable.Where(x => x.Name.FirstName.Contains(filterDriverDTO.FirstName));
        }

        if (filterDriverDTO.LastName is not null)
        {
            queryable = queryable.Where(x => x.Name.LastName.Contains(filterDriverDTO.LastName));
        }

        if (filterDriverDTO.License is not null)
        {
            var license = DriverLicense.TryCreate(filterDriverDTO.License);
            if (license is not null)
                queryable = queryable.Where(x => x.License == license);
        }

        if (filterDriverDTO.Email is not null)
        {
            var email = Email.TryCreate(filterDriverDTO.Email);
            if (email is not null)
                queryable = queryable.Where(x => x.Email == email);
        }

        if (filterDriverDTO.PhoneNumber is not null)
        {
            var phoneNumber = PhoneNumber.TryCreate(filterDriverDTO.PhoneNumber);
            if (phoneNumber is not null)
                queryable = queryable.Where(x => x.PhoneNumber == phoneNumber);
        }

        if (filterDriverDTO.DateOfBirthFrom is not null)
        {
            queryable = queryable.Where(x => x.DateOfBirth >= filterDriverDTO.DateOfBirthFrom);
        }

        if (filterDriverDTO.DateOfBirthTo is not null)
        {
            queryable = queryable.Where(x => x.DateOfBirth <= filterDriverDTO.DateOfBirthTo);
        }

        if (filterDriverDTO.Status is not null)
        {
            queryable = queryable.Where(x => x.Status == filterDriverDTO.Status);
        }

        if (filterDriverDTO.AssignedVehicleId is not null)
        {
            queryable = queryable.Where(x => x.AssignedVehicleId == filterDriverDTO.AssignedVehicleId);
        }

        return await queryable
            .OrderBy(x => x.Id)
            .Skip((filterDriverDTO.Page - 1) * filterDriverDTO.RecordsByPage)
            .Take(filterDriverDTO.RecordsByPage)
            .ToListAsync(cancellationToken);
    }
}
