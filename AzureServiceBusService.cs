using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IServiceBusService
{
    Task SendMessageAsync(string queueName, string messageBody);
    Task<List<string>> ReceiveMessagesAsync(string queueName, int maxMessages);
    Task SendMessagesBatchAsync(string queueName, List<string> messages);
    Task SendMessageByTopicAsync(string topicName, string messageBody);
    Task<List<string>> ReceiveMessagesByTopicAsync(string topicName, string subscriptionName, int maxMessages);
}

public class ServiceBusService : IServiceBusService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceBusService> _logger;
    private readonly ServiceBusClient _client;

    public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _client = new ServiceBusClient(_configuration.GetConnectionString("ServiceBusConnectionString"));
    }

    public async Task SendMessageAsync(string queueName, string messageBody)
    {
        try
        {
            var sender = _client.CreateSender(queueName);
            var message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            throw;
        }
    }

    public async Task<List<string>> ReceiveMessagesAsync(string queueName, int maxMessages)
    {
        try
        {
            var receiver = _client.CreateReceiver(queueName);
            var messages = await receiver.ReceiveMessagesAsync(maxMessages);
            var messageBodies = new List<string>();
            foreach (var message in messages)
            {
                messageBodies.Add(message.Body.ToString());
                await receiver.CompleteMessageAsync(message);
            }
            return messageBodies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving messages");
            throw;
        }
    }

    public async Task SendMessagesBatchAsync(string queueName, List<string> messages)
    {
        try
        {
            var sender = _client.CreateSender(queueName);
            var messageBatch = await sender.CreateMessageBatchAsync();

            foreach (var messageBody in messages)
            {
                var message = new ServiceBusMessage(messageBody);
                if (!messageBatch.TryAddMessage(message))
                {
                    await sender.SendMessagesAsync(messageBatch);
                    messageBatch = await sender.CreateMessageBatchAsync();
                    messageBatch.TryAddMessage(message);
                }
            }

            if (messageBatch.Count > 0)
            {
                await sender.SendMessagesAsync(messageBatch);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch of messages");
            throw;
        }
    }

    public async Task SendMessageByTopicAsync(string topicName, string messageBody)
    {
        try
        {
            var sender = _client.CreateSender(topicName);
            var message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message by topic");
            throw;
        }
    }

    public async Task<List<string>> ReceiveMessagesByTopicAsync(string topicName, string subscriptionName, int maxMessages)
    {
        try
        {
            var receiver = _client.CreateReceiver(topicName, subscriptionName);
            var messages = await receiver.ReceiveMessagesAsync(maxMessages);
            var messageBodies = new List<string>();
            foreach (var message in messages)
            {
                messageBodies.Add(message.Body.ToString());
                await receiver.CompleteMessageAsync(message);
            }
            return messageBodies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving messages by topic");
            throw;
        }
    }
}
