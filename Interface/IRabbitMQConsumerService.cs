using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace azureservicebusdemo.Interface
{
    public interface IRabbitMQConsumerService
    {
        Task StartConsumingAsync();
        bool TryDequeueMessage(out string message);
    }
}
