using Azure.Messaging.ServiceBus;

const string connectionString =
    "Endpoint=sb://localhost;" +
    "SharedAccessKeyName=RootManageSharedAccessKey;" +
    "SharedAccessKey=SAS_KEY_VALUE;" +
    "UseDevelopmentEmulator=true;";

const string queueName = "orders";

await using var client = new ServiceBusClient(connectionString);

ServiceBusSender sender = client.CreateSender(queueName);

var message = new ServiceBusMessage("Hello from .NET Producer!");

await sender.SendMessageAsync(message);

Console.WriteLine("Message sent successfully!");