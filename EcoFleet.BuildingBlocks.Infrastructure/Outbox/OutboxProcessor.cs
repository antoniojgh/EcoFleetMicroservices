using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace EcoFleet.BuildingBlocks.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    //private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);
    // AJGR temp, Delete the line below and uncomment the line above to test the processor more frequently (every 10 seconds instead of 3 minutes)
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(60);
    private const int BatchSize = 20;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        // Use an explicit transaction so UPDLOCK is held until commit
        await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

        // UPDLOCK: locks selected rows so other replicas cannot pick them up
        // READPAST: other replicas skip locked rows instead of waiting (no blocking)
        var messages = await dbContext.OutboxMessages
            .FromSqlRaw(
                """
                SELECT TOP ({0}) *
                FROM OutboxMessages WITH (UPDLOCK, READPAST)
                WHERE ProcessedOn IS NULL
                ORDER BY OccurredOn
                """,
                BatchSize)
            .ToListAsync(stoppingToken);

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(stoppingToken);
            return;
        }

        _logger.LogInformation("Processing {Count} outbox message(s).", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);

                if (eventType is null)
                {
                    _logger.LogWarning("Could not resolve type '{Type}' for outbox message {Id}.", message.Type, message.Id);
                    message.Error = $"Could not resolve type '{message.Type}'.";
                    message.ProcessedOn = DateTime.UtcNow;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;

                if (domainEvent is null)
                {
                    _logger.LogWarning("Could not deserialize outbox message {Id} to type '{Type}'.", message.Id, message.Type);
                    message.Error = $"Could not deserialize to type '{message.Type}'.";
                    message.ProcessedOn = DateTime.UtcNow;
                    continue;
                }

                await publisher.Publish(domainEvent, stoppingToken);

                message.ProcessedOn = DateTime.UtcNow;
                message.Error = null;

                _logger.LogInformation("Successfully processed outbox message {Id} of type '{Type}'.", message.Id, message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {Id}.", message.Id);
                message.Error = ex.Message;
                message.ProcessedOn = DateTime.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);
    }
}
