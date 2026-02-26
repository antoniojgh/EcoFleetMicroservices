using EcoFleet.OrderService.Application.Interfaces;
using EcoFleet.OrderService.Infrastructure.EventStore;
using EcoFleet.OrderService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services)
    {
        // 1. Register Order Event Store Repository (Marten-based)
        services.AddScoped<IOrderEventStore, OrderEventStoreRepository>();

        // 2. Register Order Read Repository (Marten read model queries)
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();

        return services;
    }
}
