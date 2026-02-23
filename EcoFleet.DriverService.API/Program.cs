using EcoFleet.DriverService.API.Middlewares;
using EcoFleet.DriverService.Application;
using EcoFleet.DriverService.Infrastructure;
using Marten;
using MassTransit;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Application Layer (MediatR + FluentValidation)
builder.Services.AddDriverApplication();

// 3. Infrastructure Layer (EF Core + own database)
builder.Services.AddDriverInfrastructure(builder.Configuration);

// 4. Marten Event Store (PostgreSQL)
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("EventStore")!);
    options.DatabaseSchemaName = "driver_events";
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
        cfg.ConfigureEndpoints(context);
    });
});

// 6. API Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 7. Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DriverDb")!)
    .AddRabbitMQ(builder.Configuration.GetConnectionString("RabbitMQ")!);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();