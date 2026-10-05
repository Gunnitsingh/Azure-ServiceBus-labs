
using OrderApi.Models;
using Shared.Data;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _dbContext;

    public OrderService(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateOrderAsync(CreateOrderRequest request)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            var message = new OutboxMessage
            {
                EventType = "OrderCreated",
                Payload = System.Text.Json.JsonSerializer.Serialize(new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    Amount = order.Amount
                }),
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.OutboxMessages.Add(message);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return order.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}