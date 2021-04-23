namespace ND.MessageBroker.RabbitMQ.Contracts
{
    public interface IQueueConfiguration
    {
        string Name { get; set; }

        string Exchange { get; set; }

        string RoutingKey { get; set; }
    }
}
