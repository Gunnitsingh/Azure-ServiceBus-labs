using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
namespace ServiceBus.Infrastructure;

public class ServiceBusProvider : IServiceBusProvider
{
    private readonly ServiceBusClient _client;

    public ServiceBusProvider(IConfiguration configuration)
    {
        var connectionString =
            configuration["ServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException(
                "Service Bus connection string is not configured.");

        _client = new ServiceBusClient(connectionString);
    }

    public async Task SendMessageAsync(
        string queueName,
        string messageBody,
        string messageId)
    {
        ServiceBusSender sender = _client.CreateSender(queueName);

        var message = new ServiceBusMessage(messageBody)
        {
            MessageId = messageId
        };

        await sender.SendMessageAsync(message);
    }
}