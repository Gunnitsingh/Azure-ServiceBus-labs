using Microsoft.EntityFrameworkCore;
using ServiceBus.Infrastructure;
using Shared.Data;

namespace OutboxPublisher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceBusProvider _serviceBusProvider;

    public Worker(
        ILogger<Worker> logger,
        IServiceScopeFactory scopeFactory,
        IServiceBusProvider serviceBusProvider)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _serviceBusProvider = serviceBusProvider;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            var messages = await dbContext.OutboxMessages
                .Where(x => x.Status == OutboxStatus.Pending)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                await _serviceBusProvider.SendMessageAsync(
                    "orders",
                    message.Payload,
                    message.Id.ToString());

                message.Status = OutboxStatus.Published;
                message.ProcessedAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation(
                    "Published Outbox message {MessageId}",
                    message.Id);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}