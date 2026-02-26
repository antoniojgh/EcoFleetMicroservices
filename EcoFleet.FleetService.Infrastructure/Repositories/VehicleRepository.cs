using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.FleetService.Application.DTOs;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Entities;
using EcoFleet.FleetService.Domain.ValueObjects;
using EcoFleet.FleetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.FleetService.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly FleetDbContext _context;

    public VehicleRepository(FleetDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> GetByIdAsync(VehicleId id, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles.ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalNumberOfRecords(CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Vehicle entity, CancellationToken cancellationToken = default)
    {
        await _context.Vehicles.AddAsync(entity, cancellationToken);
    }

    public Task Update(Vehicle entity, CancellationToken cancellationToken = default)
    {
        _context.Vehicles.Update(entity);
        return Task.CompletedTask;
    }

    public Task Delete(Vehicle entity, CancellationToken cancellationToken = default)
    {
        _context.Vehicles.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .Where(v => v.CurrentDriverId == driverId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vehicle>> GetFilteredAsync(FilterVehicleDTO filterVehicleDTO, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Vehicles.AsQueryable();

        if (filterVehicleDTO.Id is not null)
        {
            var vehicleId = new VehicleId(filterVehicleDTO.Id.Value);
            queryable = queryable.Where(x => x.Id == vehicleId);
        }

        if (filterVehicleDTO.Plate is not null)
        {
            var plate = LicensePlate.TryCreate(filterVehicleDTO.Plate);
            if (plate is not null)
                queryable = queryable.Where(x => x.Plate == plate);
        }

        if (filterVehicleDTO.Status is not null)
        {
            queryable = queryable.Where(x => x.Status == filterVehicleDTO.Status);
        }

        if (filterVehicleDTO.CurrentDriverId is not null)
        {
            queryable = queryable.Where(x => x.CurrentDriverId == filterVehicleDTO.CurrentDriverId);
        }

        var totalRecords = await queryable.CountAsync(cancellationToken);

        var results = await queryable
            .Skip((filterVehicleDTO.Page - 1) * filterVehicleDTO.RecordsByPage)
            .Take(filterVehicleDTO.RecordsByPage)
            .ToListAsync(cancellationToken);

        return results;
    }
}
