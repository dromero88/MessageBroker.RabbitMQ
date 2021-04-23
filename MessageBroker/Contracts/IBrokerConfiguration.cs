using ND.MessageBroker.RabbitMQ.Configurations;
using System.Collections.Generic;

namespace ND.MessageBroker.RabbitMQ.Contracts
{
    public interface IBrokerConfiguration
    {
        string Username { get; set; }

        string Password { get; set; }

        int Port { get; set; }

        string VirtualHost { get; set; }

        string HostName { get; set; }

        List<QueueConfiguration> Subscriptions { get;set;}
    }
}
