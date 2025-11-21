// RabbitMQConfig.cs
public class RabbitMQConfig
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "order_exchange";
    public string QueueName { get; set; } = "order_queue";
    public string RoutingKey { get; set; } = "order.created";
}