using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Infrastructure.EventStore;
using EcoFleet.DriverService.Infrastructure.Outbox;
using EcoFleet.DriverService.Infrastructure.Persistence;
using EcoFleet.DriverService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.DriverService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDriverInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DriverDb");

        // 1. Add DbContext (Database-per-Service)
        services.AddDbContext<DriverDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 2. Register Driver Repository
        services.AddScoped<IDriverRepository, DriverRepository>();

        // 3. Register Driver Event Store Repository (Marten-based)
        services.AddScoped<IDriverEventStore, DriverEventStoreRepository>();

        // 4. Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 5. Register Outbox Processor (Background Worker)
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
