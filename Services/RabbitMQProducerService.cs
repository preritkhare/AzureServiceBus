using azureservicebusdemo.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

public class RabbitMQProducerService
{
    private readonly RabbitMQSettings _settings;

    public RabbitMQProducerService(IOptions<RabbitMQSettings> settings)
    {
        _settings = settings.Value;
    }

    public void SendMessage(string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _settings.QueueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: _settings.QueueName,
                             basicProperties: null,
                             body: body);
    }
}
