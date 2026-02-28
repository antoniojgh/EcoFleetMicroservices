using EcoFleet.OrderService.API.Middlewares;
using EcoFleet.OrderService.Application;
using EcoFleet.OrderService.Infrastructure;
using EcoFleet.OrderService.Infrastructure.Projections;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using MassTransit;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Application Layer (MediatR + FluentValidation)
builder.Services.AddOrderApplication();

// 3. Infrastructure Layer (Event Store + Read Repository)
builder.Services.AddOrderInfrastructure();

// 4. Marten Event Store (PostgreSQL)
builder.Services.AddMarten(sp =>
{
    var options = new StoreOptions();
    options.Connection(builder.Configuration.GetConnectionString("EventStore")!);
    options.DatabaseSchemaName = "order_events";
    options.Projections.Add<OrderReadModelProjection>(ProjectionLifecycle.Inline);
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
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));

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
    .AddNpgSql(builder.Configuration.GetConnectionString("EventStore")!, name: "postgres")
    .AddRabbitMQ(sp => new ConnectionFactory
    {
        Uri = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)
    }.CreateConnectionAsync().GetAwaiter().GetResult(), name: "rabbitmq");

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
