using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.ManagerService.Application.Interfaces;
using EcoFleet.ManagerService.Infrastructure.Persistence;
using EcoFleet.ManagerService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.ManagerService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddManagerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ManagerDb");

        // 1. Add DbContext (Database-per-Service)
        services.AddDbContext<ManagerDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 2. Register Manager Repository
        services.AddScoped<IManagerRepository, ManagerRepository>();

        // 3. Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
