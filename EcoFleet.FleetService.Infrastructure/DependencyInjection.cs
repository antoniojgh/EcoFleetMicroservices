using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Infrastructure.Outbox;
using EcoFleet.FleetService.Infrastructure.Persistence;
using EcoFleet.FleetService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.FleetService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFleetInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FleetDb");

        // 1. Add DbContext (Database-per-Service)
        services.AddDbContext<FleetDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 2. Register Vehicle Repository
        services.AddScoped<IVehicleRepository, VehicleRepository>();

        // 3. Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 4. Register Outbox Processor (Background Worker)
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
