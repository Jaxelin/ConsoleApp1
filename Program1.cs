using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 配置 RabbitMQ
builder.Services.AddSingleton<RabbitMQConfig>(sp => new RabbitMQConfig
{
    HostName = "localhost",
    ExchangeName = "my_exchange",
    QueueName = "my_queue"
});

// 注册 RabbitMQ 服务
builder.Services.AddSingleton<RabbitMQService>();

var host = builder.Build();

// 在后台服务中使用
await using var scope = host.Services.CreateAsyncScope();
var rabbitService = scope.ServiceProvider.GetRequiredService<RabbitMQService>();

try
{
    // 发送消息 (异步)
    var order = new OrderMessage
    {
        OrderId = $"ORD-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
        CustomerId = "CUST-123",
        Amount = 99.99m,
        CreatedAt = DateTime.UtcNow
    };

    await rabbitService.PublishAsync(order, "order.created");
    Console.WriteLine("Message published asynchronously.");

    // 开始消费消息 (异步)
    _ = rabbitService.StartConsumingAsync();

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
finally
{
    // 清理资源
    await rabbitService.DisposeAsync();
}