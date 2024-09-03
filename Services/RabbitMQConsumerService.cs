using azureservicebusdemo.Interface;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

public class RabbitMQConsumerService : IRabbitMQConsumerService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
    private readonly ILogger<RabbitMQConsumerService> _logger;

    public RabbitMQConsumerService(ConnectionFactory connectionFactory, ILogger<RabbitMQConsumerService> logger)
    {
        _logger = logger;

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare the queue (if not already declared)
        _channel.QueueDeclare(queue: "myQueue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // Start consuming messages
        StartConsumingAsync().ConfigureAwait(false);
    }

    public async Task StartConsumingAsync()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Received message: {message}");
            _messageQueue.Enqueue(message);
            _logger.LogInformation("Consumer started.");
        };

        _channel.BasicConsume(queue: "myQueue",
                             autoAck: true,
                             consumer: consumer);

        _logger.LogInformation("Consumer not started.");
    }

    public bool TryDequeueMessage(out string message)
    {
        return _messageQueue.TryDequeue(out message);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
