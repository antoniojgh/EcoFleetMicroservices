using EcoFleet.BuildingBlocks.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.DriverService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDriverApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // 1. Register MediatR
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            // Register the Validation Pipeline
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // 2. Register Validators
        services.AddValidatorsFromAssembly(assembly);

        // 3. Register Logging Pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
