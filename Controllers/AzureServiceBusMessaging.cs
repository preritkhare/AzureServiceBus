using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace azureservicebusdemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AzureServiceBusMessagingController : ControllerBase
    {
        private readonly ILogger<AzureServiceBusMessagingController> _logger;
        private readonly IServiceBusService _serviceBusService;

        public AzureServiceBusMessagingController(ILogger<AzureServiceBusMessagingController> logger, IServiceBusService serviceBusService)
        {
            _logger = logger;
            _serviceBusService = serviceBusService;
        }

        [HttpPost("sendmessagebyQueue")]
        public async Task<IActionResult> SendMessageAsync([FromQuery] string queueName, [FromBody] string messageBody)
        {
            if (string.IsNullOrEmpty(queueName) || string.IsNullOrEmpty(messageBody))
            {
                return BadRequest("Queue name and message body are required.");
            }

            try
            {
                await _serviceBusService.SendMessageAsync(queueName, messageBody);
                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("receivemessagefromQueue")]
        public async Task<IActionResult> ReceiveMessagesAsync([FromQuery] string queueName, [FromQuery] int maxMessages = 10)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                return BadRequest("Queue name is required.");
            }

            try
            {
                var messages = await _serviceBusService.ReceiveMessagesAsync(queueName, maxMessages);
                if (messages.Count == 0)
                {
                    return NoContent();
                }
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("sendbatchmessagebyQueue")]
        public async Task<IActionResult> SendQueueMessagesBatchAsync([FromBody] List<string> messages, [FromQuery] string queueName)
        {
            if (messages == null || messages.Count == 0 || string.IsNullOrEmpty(queueName))
            {
                return BadRequest("Queue name and messages are required.");
            }

            try
            {
                await _serviceBusService.SendMessagesBatchAsync(queueName, messages);
                return Ok("Messages sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending batch of messages");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("sendmessagebyTopic")]
        public async Task<IActionResult> SendMessageByTopicAsync([FromQuery] string topicName, [FromBody] string messageBody)
        {
            if (string.IsNullOrEmpty(topicName) || string.IsNullOrEmpty(messageBody))
            {
                return BadRequest("Topic name and message body are required.");
            }

            try
            {
                await _serviceBusService.SendMessageByTopicAsync(topicName, messageBody);
                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message by topic");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("receivemessagesbyTopic")]
        public async Task<IActionResult> ReceiveMessagesFromTopicAsync([FromQuery] string topicName, [FromQuery] string subscriptionName, [FromQuery] int maxMessages = 10)
        {
            if (string.IsNullOrEmpty(topicName) || string.IsNullOrEmpty(subscriptionName))
            {
                return BadRequest("Topic name and subscription name are required.");
            }

            try
            {
                var messages = await _serviceBusService.ReceiveMessagesByTopicAsync(topicName, subscriptionName, maxMessages);
                if (messages.Count == 0)
                {
                    return NoContent();
                }
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages by topic");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
