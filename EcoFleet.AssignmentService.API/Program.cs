using EcoFleet.AssignmentService.API.Middlewares;
using EcoFleet.AssignmentService.Application;
using EcoFleet.AssignmentService.Infrastructure;
using EcoFleet.AssignmentService.Infrastructure.Persistence;
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
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));

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
        Uri = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)
    }.CreateConnectionAsync().GetAwaiter().GetResult());

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure SQL Server database schema is created (no migration files needed)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AssignmentDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
