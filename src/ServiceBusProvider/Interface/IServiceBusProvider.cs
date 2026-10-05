namespace ServiceBus.Infrastructure;

public interface IServiceBusProvider
{
    Task SendMessageAsync(
        string queueName,
        string messageBody,
        string messageId);
}