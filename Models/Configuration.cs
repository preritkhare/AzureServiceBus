using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace azureservicebusdemo.Models
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string QueueName { get; set; } = "hello";
    }
}
