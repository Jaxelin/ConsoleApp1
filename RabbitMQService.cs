using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client.Exceptions;

/// <summary>
/// 用于与 RabbitMQ 进行交互的服务类 (7.2.0 ver.)
/// </summary>
public class RabbitMQService : IAsyncDisposable
{
    private Task _initializeTask;
    private readonly IConnection _connection;
    private readonly RabbitMQConfig _config;
    private IChannel? _consumerChannel;

    public RabbitMQService(RabbitMQConfig config)
    {
        
        _config = config;

        var factory = new ConnectionFactory
        {
            HostName = config.HostName,
            Port = config.Port,
            UserName = config.UserName,
            Password = config.Password,
            //DispatchConsumersAsync = true, // 启用异步消费者，7.2.0 版本取消了该属性，默认即为 true
            AutomaticRecoveryEnabled = true, // 启用自动恢复
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        // 创建连接 (异步)
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();

        // 初始化交换机和队列
        _initializeTask = InitializeBrokerAsync();
    }

    private async Task InitializeBrokerAsync()
    {
        await using var channel = await _connection.CreateChannelAsync();

        // 声明交换机
        await channel.ExchangeDeclareAsync(
            exchange: _config.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null);

        // 声明队列
        await channel.QueueDeclareAsync(
            queue: _config.QueueName,
            exclusive: false,
            durable: true,
            autoDelete: false,
            arguments: null);

        // 绑定队列到交换机
        await channel.QueueBindAsync(
            queue: _config.QueueName,
            exchange: _config.ExchangeName,
            routingKey: _config.RoutingKey,
            arguments: null);
    }

    /// <summary>
    /// 创建并初始化 RabbitMQ 代理 (完全异步，7.2.0 兼容)
    /// </summary>
    /// <returns></returns>
    public async Task CreateAsync() => await _initializeTask;


    /// <summary>
    /// 发布消息到指定路由键 (完全异步，7.2.0 兼容)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="routingKey"></param>
    /// <returns></returns>
    public async Task PublishAsync<T>(T message, string routingKey)
    {
        await using var channel = await _connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // ✅ 7.2.0 正确创建 BasicProperties
        var properties = new BasicProperties
        {
            Persistent = true, // 持久化消息 (RabbitMQ 7.0+ 中此属性替代 DeliveryMode)
            ContentType = "application/json",
            ContentEncoding = "UTF-8",
            Headers = new Dictionary<string, object?>
            {
                { "message-type", typeof(T).Name },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            }
        };

        await channel.BasicPublishAsync(
            exchange: _config.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);

        Console.WriteLine($"✅ Published message to {routingKey}");
    }

    // 消费消息 (完全异步，7.2.0 兼容)
    public async Task StartConsumingAsync(Func<OrderMessage, Task> messageHandler)
    {
        // 消费者通道需要长期保持，不能使用 using 语句
        _consumerChannel = await _connection.CreateChannelAsync();

        // ✅ 7.2.0 正确设置 QoS (预取计数)
        await _consumerChannel.BasicQosAsync(
            prefetchSize: 0,    // 0 表示不限制大小
            prefetchCount: 10,  // 每次预取 10 条消息
            global: false);     // 仅应用于当前消费者

        // ✅ 7.2.0 使用 AsyncEventingBasicConsumer
        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var body = args.Body.Span.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"📥 Received message: {message}");

                // 反序列化消息
                var order = JsonSerializer.Deserialize<OrderMessage>(message)
                           ?? throw new InvalidOperationException("Failed to deserialize message");

                // 处理消息
                await messageHandler(order);

                // ✅ 7.2.0 确认消息 (异步)
                await _consumerChannel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);

                Console.WriteLine($"✅ Processed and acknowledged message {args.DeliveryTag}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing message: {ex.Message}");

                try
                {
                    // 拒绝消息，不重新入队
                    await _consumerChannel.BasicRejectAsync(
                        deliveryTag: args.DeliveryTag,
                        requeue: false);

                    Console.WriteLine($"❌ Rejected message {args.DeliveryTag}");
                }
                catch (AlreadyClosedException)
                {
                    Console.WriteLine("Channel already closed, cannot reject message");
                }
                catch (Exception rejectEx)
                {
                    Console.WriteLine($"❌ Error rejecting message: {rejectEx.Message}");
                }
            }
        };

        // ✅ 7.2.0 开始消费 (异步)
        await _consumerChannel.BasicConsumeAsync(
            queue: _config.QueueName,
            autoAck: false, // 手动确认
            consumer: consumer,
            consumerTag: $"consumer-{Guid.NewGuid().ToString("N").Substring(0, 8)}");

        Console.WriteLine("🎯 Started consuming messages asynchronously...");
    }

    private async Task ProcessOrderAsync(OrderMessage order)
    {
        Console.WriteLine($"🔄 Processing order {order.OrderId} for customer {order.CustomerId}");
        // 模拟业务处理
        await Task.Delay(200);
        Console.WriteLine($"✅ Completed processing order {order.OrderId}");
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            // 关闭消费者通道
            if (_consumerChannel != null && _consumerChannel.IsOpen)
            {
                await _consumerChannel.CloseAsync();
                await _consumerChannel.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error closing consumer channel: {ex.Message}");
        }
        finally
        {
            _consumerChannel = null;
        }

        try
        {
            // 关闭连接
            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error closing connection: {ex.Message}");
        }
    }
}