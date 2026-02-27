using EcoFleet.AssignmentService.API.Middlewares;
using EcoFleet.AssignmentService.Application;
using EcoFleet.AssignmentService.Infrastructure;
using MassTransit;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Application Layer (MediatR + FluentValidation)
builder.Services.AddAssignmentApplication();

// 3. Infrastructure Layer (EF Core + own database)
builder.Services.AddAssignmentInfrastructure(builder.Configuration);

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
    .AddSqlServer(builder.Configuration.GetConnectionString("AssignmentDb")!)
    .AddRabbitMQ(sp => new ConnectionFactory
    {
        Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")!)
    }.CreateConnectionAsync().GetAwaiter().GetResult());

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
