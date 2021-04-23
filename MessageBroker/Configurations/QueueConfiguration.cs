using ND.MessageBroker.RabbitMQ.Contracts;

namespace ND.MessageBroker.RabbitMQ.Configurations
{
    public class QueueConfiguration : IQueueConfiguration
    {
        public string Name { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
    }
}
