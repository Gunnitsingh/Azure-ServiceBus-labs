using Azure.Messaging.ServiceBus;
using Microsoft.Data.SqlClient;

const string connectionString =
    "Endpoint=sb://localhost;" +
    "SharedAccessKeyName=RootManageSharedAccessKey;" +
    "SharedAccessKey=SAS_KEY_VALUE;" +
    "UseDevelopmentEmulator=true;";

const string sqlConnectionString =
    "Server=localhost,1433;" +
    "User Id=sa;" +
    "Password=ServiceBusLab@12345;" +
    "TrustServerCertificate=True;" +
    "Database=ServiceBusLab;";

const string queueName = "orders";

await using var sqlConnection = new SqlConnection(sqlConnectionString);

await sqlConnection.OpenAsync();


await using var client = new ServiceBusClient(connectionString);

ServiceBusProcessor processor = client.CreateProcessor(
    queueName,
    new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false
    });

processor.ProcessMessageAsync += async args =>
{
    var messageId = args.Message.MessageId;

    Console.WriteLine($"Received: {args.Message.Body}");
    Console.WriteLine($"MessageId: {messageId}");

    await using var connection =
        new SqlConnection(sqlConnectionString);

    await connection.OpenAsync();

    await using var transaction =
    (SqlTransaction)await connection.BeginTransactionAsync();

    // 1. Check whether this message was already processed
    await using var checkCommand = connection.CreateCommand();

    checkCommand.Transaction = transaction;

    checkCommand.CommandText = """
        SELECT COUNT(1)
        FROM ProcessedMessages
        WHERE MessageId = @MessageId
        """;

    checkCommand.Parameters.AddWithValue("@MessageId", messageId);

    var result = await checkCommand.ExecuteScalarAsync();

    var alreadyProcessed = Convert.ToInt32(result);

    if (alreadyProcessed > 0)
    {
        Console.WriteLine("Message already processed. Skipping business operation.");

        await transaction.CommitAsync();

        await args.CompleteMessageAsync(args.Message);

        return;
    }

    // 2. Business operation
    Console.WriteLine("Processing order...");

    // Simulate updating business data
    await Task.Delay(500);

    Console.WriteLine("Business operation completed.");

    // 3. Record the message as processed
    await using var insertCommand = connection.CreateCommand();

    insertCommand.Transaction = transaction;

    insertCommand.CommandText = """
        INSERT INTO ProcessedMessages (MessageId, ProcessedAt)
        VALUES (@MessageId, SYSUTCDATETIME())
        """;

    insertCommand.Parameters.AddWithValue("@MessageId", messageId);

    await insertCommand.ExecuteNonQueryAsync();

    // 4. Commit SQL transaction
    await transaction.CommitAsync();

    Console.WriteLine("Idempotency record committed.");

    Console.WriteLine("💥 Simulating crash BEFORE Service Bus Complete!");

    Environment.Exit(1);
};

processor.ProcessErrorAsync += args =>
{
    Console.WriteLine($"Error: {args.Exception.Message}");
    return Task.CompletedTask;
};

await processor.StartProcessingAsync();

Console.WriteLine("Consumer started. Press Enter to stop.");
Console.ReadLine();

await processor.StopProcessingAsync();