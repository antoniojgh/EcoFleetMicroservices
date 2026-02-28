using EcoFleet.NotificationService.API.Notifications;
using MassTransit;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Notification Service (email sending)
builder.Services.AddScoped<INotificationsService, NotificationsService>();

// 3. MassTransit + RabbitMQ
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

// 4. Health Checks
builder.Services.AddHealthChecks()
    .AddRabbitMQ(sp => new ConnectionFactory
    {
        Uri = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)
    }.CreateConnectionAsync().GetAwaiter().GetResult());

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");

app.Run();
