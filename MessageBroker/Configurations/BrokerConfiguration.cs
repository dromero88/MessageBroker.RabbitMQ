using ND.MessageBroker.RabbitMQ.Contracts;
using System.Collections.Generic;

namespace ND.MessageBroker.RabbitMQ.Configurations
{
    public class BrokerConfiguration: IBrokerConfiguration
    {
        public string Username { get; set; }

        public string Password { get; set; }
        
        public int? Port { get; set; }

        public string VirtualHost { get; set; }

        public string HostName { get; set; }
        public List<QueueConfiguration> Subscriptions { get; set; }
    }
}
