using Microsoft.EntityFrameworkCore;
using OutboxPublisher;
using Shared.Data;
using ServiceBus.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrderDatabase")));
builder.Services.AddSingleton<IServiceBusProvider, ServiceBusProvider>();
var host = builder.Build();
host.Run();
