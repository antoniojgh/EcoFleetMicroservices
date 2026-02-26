using EcoFleet.AssignmentService.Application.Interfaces;
using EcoFleet.AssignmentService.Infrastructure.Outbox;
using EcoFleet.AssignmentService.Infrastructure.Persistence;
using EcoFleet.AssignmentService.Infrastructure.Repositories;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.AssignmentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAssignmentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AssignmentDb");

        // 1. Add DbContext (Database-per-Service)
        services.AddDbContext<AssignmentDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 2. Register Assignment Repository
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();

        // 3. Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 4. Register Outbox Processor (Background Worker)
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
