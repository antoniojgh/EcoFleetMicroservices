using EcoFleet.FleetService.API.Middlewares;
using EcoFleet.FleetService.Application;
using EcoFleet.FleetService.Infrastructure;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Application Layer (MediatR + FluentValidation)
builder.Services.AddFleetApplication();

// 3. Infrastructure Layer (EF Core + own database)
builder.Services.AddFleetInfrastructure(builder.Configuration);

// Add services to the container.

// 4. MassTransit + RabbitMQ
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

// 5. API Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 6. Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("FleetDb")!)
    .AddRabbitMQ(rabbitConnectionString: builder.Configuration.GetConnectionString("RabbitMQ")!);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
