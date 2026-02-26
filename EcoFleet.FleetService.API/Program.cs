using EcoFleet.FleetService.API.Middlewares;
using EcoFleet.FleetService.Application;
using EcoFleet.FleetService.Infrastructure;
using EcoFleet.FleetService.Infrastructure.Projections;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Application Layer (MediatR + FluentValidation)
builder.Services.AddFleetApplication();

// 3. Infrastructure Layer (EF Core + own database)
builder.Services.AddFleetInfrastructure(builder.Configuration);

// 4. Marten Event Store (PostgreSQL)
builder.Services.AddMarten(sp =>
{
    var options = new StoreOptions();
    options.Connection(builder.Configuration.GetConnectionString("EventStore")!);
    options.DatabaseSchemaName = "fleet_events";
    options.Projections.Add<VehicleReadModelProjection>(ProjectionLifecycle.Inline);
    return options;
}).UseLightweightSessions();

// 5. MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    // Register all consumers from this assembly
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        // Enable retries with incremental backoff
        cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));

        cfg.ConfigureEndpoints(context);
    });
});

// 6. API Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 7. Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("FleetDb")!);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
