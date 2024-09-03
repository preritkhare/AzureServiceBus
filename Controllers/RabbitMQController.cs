using azureservicebusdemo.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace azureservicebusdemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RabbitMQController : Controller
    {
        private readonly RabbitMQProducerService _producerService;
        private readonly IRabbitMQConsumerService _consumerService;

        public RabbitMQController(RabbitMQProducerService producerService, IRabbitMQConsumerService consumerService)
        {
            _producerService = producerService;
            _consumerService = consumerService;
        }

        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] string message)
        {
            _producerService.SendMessage(message);
            return Ok(new { Status = "Message Sent" });
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartConsuming()
        {
            try
            {
                await _consumerService.StartConsumingAsync();
                return Ok(new { Status = "Consuming started" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("messages")]
        public IActionResult GetMessages()
        {
            if (_consumerService.TryDequeueMessage(out var message))
            {
                return Ok(new { Message = message });
            }
            else
            {
                return NoContent();
            }
        }

      
    }
}
