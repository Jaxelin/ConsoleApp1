using RabbitMQ.Client.Exceptions;

namespace ConsoleApp1
{
    internal class Program1
    {
        static async Task Main1(string[] args)
        {
            int[] c = new int[5];
            // 配置
            var config = new RabbitMQConfig
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                ExchangeName = "order_exchange_v2",
                QueueName = "order_queue_v2",
                RoutingKey = "order.created"
            };

            // 创建服务
            await using var rabbitService = new RabbitMQService(config);
            await rabbitService.CreateAsync();
            try
            {
                // 发布多条消息
                for (int i = 0; i < 5; i++)
                {
                    var order = new OrderMessage
                    {
                        OrderId = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{i:D3}",
                        CustomerId = $"CUST-{i + 1}",
                        Amount = 100.00m * (i + 1),
                        CreatedAt = DateTime.UtcNow
                    };

                    await rabbitService.PublishAsync(order, "order.created");
                    await Task.Delay(100); // 间隔 100ms
                }

                // 启动消费者
                await rabbitService.StartConsumingAsync(order =>
                {
                    Console.WriteLine($"💼 Business processing order: {order.OrderId}");
                    return Task.CompletedTask;
                });

                Console.WriteLine("🚀 All services started. Press any key to exit...");
                Console.ReadKey();
            }
            catch (BrokerUnreachableException ex)
            {
                Console.WriteLine($"💔 Cannot connect to RabbitMQ: {ex.Message}");
                Console.WriteLine("Please ensure RabbitMQ server is running at localhost:5672");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Unexpected error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                await rabbitService.DisposeAsync();
            }
        }
    }
}
