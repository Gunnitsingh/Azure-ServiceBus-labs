using Microsoft.EntityFrameworkCore;
using OrderApi.Models;
using Shared.Models;

namespace Shared.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ProcessedMessage>()
            .HasKey(x => x.MessageId);
    }
}